﻿using System;
using Fudge;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;

namespace OGDotNet.Model.Resources
{
    public class RemoteHistoricalDataSource
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _rest;

        public RemoteHistoricalDataSource(OpenGammaFudgeContext fudgeContext, RestTarget rest)
        {
            _fudgeContext = fudgeContext;
            _rest = rest;
        }

        public ILocalDateDoubleTimeSeries GetHistoricalData(UniqueIdentifier uid)
        {
            RestTarget target = _rest.Resolve("uid")
                .Resolve(uid.ToString());
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        public ILocalDateDoubleTimeSeries GetHistoricalData(UniqueIdentifier uid, DateTimeOffset start, bool inclusiveStart, DateTimeOffset end, bool exclusiveEnd)
        {
            RestTarget target = _rest.Resolve("uidByDate")
                                      .Resolve(uid.ToString())
                                      .Resolve(UriEncoding.ToString(start))
                                      .Resolve(inclusiveStart.ToString())
                                      .Resolve(UriEncoding.ToString(end))
                                      .Resolve(exclusiveEnd.ToString());
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        
        public Tuple<UniqueIdentifier, ILocalDateDoubleTimeSeries> GetHistoricalData(IdentifierBundle identifiers, String configDocName=null)
        {
            return GetHistoricalData(identifiers, default(DateTimeOffset), configDocName);
        }

        public Tuple<UniqueIdentifier, ILocalDateDoubleTimeSeries> GetHistoricalData(IdentifierBundle identifiers, DateTimeOffset currentDate, string configDocName)
        {
            RestTarget target = _rest.Resolve("default")
                                      .Resolve((currentDate != default(DateTimeOffset)) ? UriEncoding.ToString(currentDate) : "null")
                                      .Resolve(configDocName ?? "null", UriEncoding.GetParameters(identifiers));
            return DecodePairMessage(target.GetFudge());
        }

        private Tuple<UniqueIdentifier, ILocalDateDoubleTimeSeries> DecodePairMessage(FudgeMsg message)
        {
            if (message == null)
            {
                return new Tuple<UniqueIdentifier, ILocalDateDoubleTimeSeries>(null, null);
            }

            var uniqueIdString = message.GetString("uniqueId");
            if (uniqueIdString == null)
            {
                throw new ArgumentException("uniqueId not present in message");
            }
            var timeSeriesField = (FudgeMsg)message.GetMessage("timeSeries");
            if (timeSeriesField == null)
            {
                throw new ArgumentException("timeSeries not present in message");
            }
            return Tuple.Create(UniqueIdentifier.Parse(uniqueIdString), _fudgeContext.GetSerializer().Deserialize<ILocalDateDoubleTimeSeries>(timeSeriesField));
        }

        
    }
}
