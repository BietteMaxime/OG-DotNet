using System;
using System.Collections.Generic;

namespace OGDotNet.Mappedtypes.Util.Timeseries.Localdate
{
    public interface ILocalDateDoubleTimeSeries
    {
        LocalDateEpochDaysConverter DateTimeConverter { get; }
        IList<Tuple<DateTimeOffset, double>> Values { get; }
    }
}