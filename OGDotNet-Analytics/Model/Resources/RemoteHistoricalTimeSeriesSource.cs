//-----------------------------------------------------------------------
// <copyright file="RemoteHistoricalTimeSeriesSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            RestTarget target = _rest.Resolve("hts")
                .Resolve(uid.ObjectID.ToString(), uid.IsVersioned ? new[] { Tuple.Create("version", uid.Version) } : new Tuple<string, string>[] { });
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        public ILocalDateDoubleTimeSeries GetHistoricalTimeSeries(UniqueId uid, DateTimeOffset start, bool includeStart, DateTimeOffset end, bool includeEnd)
        {
            var args = new List<Tuple<string, string>>();
            if (uid.IsVersioned)
            {
                args.Add(Tuple.Create(uid.ToString(), uid.Version));
            }
            if (start != default(DateTimeOffset))
            {
                args.Add(Tuple.Create("start", DateToString(start.Date)));
                args.Add(Tuple.Create("includeStart", includeStart.ToString()));
            }
            if (end != default(DateTimeOffset))
            {
                args.Add(Tuple.Create("end", DateToString(end.Date)));
                args.Add(Tuple.Create("includeEnd", includeEnd.ToString()));
            }
            RestTarget target = _rest.Resolve("hts")
                                      .Resolve(uid.ObjectID.ToString(), args.ToArray());
            return target.Get<ILocalDateDoubleTimeSeries>("timeSeries");
        }

        private static string DateToString(DateTimeOffset date)
        {
            //Matches LocalDate.parse
            return string.Format("{0}-{1:00}-{2:00}", date.Year, date.Month, date.Day);
        }
        public Tuple<UniqueId, ILocalDateDoubleTimeSeries> GetHistoricalTimeSeries(ExternalIdBundle identifierBundle, DateTimeOffset identifierValidityDate, string dataSource, string dataProvider, string dataField)
        {
            return GetHistoricalTimeSeries(identifierBundle, identifierValidityDate, dataSource, dataProvider, dataField, default(DateTimeOffset), false, default(DateTimeOffset), true);
        }

        public Tuple<UniqueId, ILocalDateDoubleTimeSeries> GetHistoricalTimeSeries(ExternalIdBundle identifierBundle, DateTimeOffset identifierValidityDate, string dataSource, string dataProvider, string dataField, DateTimeOffset start, bool includeStart, DateTimeOffset end, bool includeEnd)
        {
            RestTarget target = _rest.Resolve("htsSearches", "single");
            target = identifierBundle.Identifiers.Aggregate(target, (current, id) => current.WithParam("id", id.ToString()));

            target = target.WithParam("idValidityDate", identifierValidityDate != default(DateTimeOffset) ? EncodeDate(identifierValidityDate) : "ALL");
            if (dataSource != null)
            {
                target = target.WithParam("dataSource", dataSource);
            }
            if (dataProvider != null)
            {
                target = target.WithParam("dataProvider", dataProvider);
            }
            if (dataField != null)
            {
                target = target.WithParam("dataField", dataField);
            }
            if (start != default(DateTimeOffset))
            {
                target = target.WithParam("start", EncodeDate(start));
                target = target.WithParam("includeStart", includeStart);
            }
            if (end != default(DateTimeOffset))
            {
                target = target.WithParam("end", EncodeDate(end));
                target = target.WithParam("includeEnd", includeEnd);
            }
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
