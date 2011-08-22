//-----------------------------------------------------------------------
// <copyright file="RemoteHistoricalTimeSeriesSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemoteHistoricalTimeSeriesSource
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _rest;

        public RemoteHistoricalTimeSeriesSource(OpenGammaFudgeContext fudgeContext, RestTarget rest)
        {
            _fudgeContext = fudgeContext;
            _rest = rest;
        }

        public ILocalDateDoubleTimeSeries GetHistoricalTimeSeries(UniqueId uid)
        {
            RestTarget target = _rest.Resolve("uid")
                .Resolve(uid.ToString());
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        public ILocalDateDoubleTimeSeries GetHistoricalTimeSeries(UniqueId uid, DateTimeOffset start, bool inclusiveStart, DateTimeOffset end, bool includeEnd)
        {
            RestTarget target = _rest.Resolve("uidByDate")
                                      .Resolve(uid.ToString())
                                      .Resolve(UriEncoding.ToString(start))
                                      .Resolve(inclusiveStart.ToString())
                                      .Resolve(UriEncoding.ToString(end))
                                      .Resolve(includeEnd.ToString());
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        public Tuple<UniqueId, ILocalDateDoubleTimeSeries> GetHistoricalTimeSeries(ExternalIdBundle identifierBundle, DateTimeOffset identifierValidityDate, string dataSource, string dataProvider, string dataField)
        {
            RestTarget target = _rest.Resolve("all")
                          .Resolve(EncodeDate(identifierValidityDate))
                          .Resolve(dataSource)
                          .Resolve(dataProvider ?? "null")
                          .Resolve(dataField,
                            UriEncoding.GetParameters(identifierBundle));
            return DecodePairMessage(target.GetFudge());
        }

        public Tuple<UniqueId, ILocalDateDoubleTimeSeries> GetHistoricalTimeSeries(ExternalIdBundle identifierBundle, DateTimeOffset identifierValidityDate, string dataSource, string dataProvider, string dataField, DateTimeOffset start, bool inclusiveStart, DateTimeOffset end, bool includeEnd)
        {
            ArgumentChecker.NotNull(identifierBundle, "identifierBundle");
            ArgumentChecker.NotNull(dataSource, "dataSource");
            ArgumentChecker.NotNull(dataField, "dataField");
            
            RestTarget target = _rest.Resolve("allByDate")
                                .Resolve(EncodeDate(identifierValidityDate))
                                .Resolve(dataSource)
                                .Resolve(dataProvider ?? "null")
                                .Resolve(dataField)
                                .Resolve(EncodeDate(start))
                                .Resolve(inclusiveStart.ToString())
                                .Resolve(EncodeDate(end))
                                .Resolve(includeEnd.ToString(), UriEncoding.GetParameters(identifierBundle));
            return DecodePairMessage(target.GetFudge());
        }

        private Tuple<UniqueId, ILocalDateDoubleTimeSeries> DecodePairMessage(FudgeMsg message)
        {
            if (message == null)
            {
                return new Tuple<UniqueId, ILocalDateDoubleTimeSeries>(null, null);
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
            return Tuple.Create(UniqueId.Parse(uniqueIdString), _fudgeContext.GetSerializer().Deserialize<ILocalDateDoubleTimeSeries>(timeSeriesField));
        }

        private static string EncodeDate(DateTimeOffset currentDate)
        {
            return (currentDate != default(DateTimeOffset)) ? UriEncoding.ToString(currentDate) : "null";
        }
    }
}
