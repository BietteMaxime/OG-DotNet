using System;
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
    }
}
