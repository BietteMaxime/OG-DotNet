using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class YieldCurveSnapshot
    {
        private readonly Dictionary<Identifier, ValueSnapshot> _values;
        private readonly DateTimeOffset _valuationTime;

        public YieldCurveSnapshot(Dictionary<Identifier, ValueSnapshot> values, DateTimeOffset valuationTime)
        {
            _values = values;
            _valuationTime = valuationTime;
        }

        public Dictionary<Identifier, ValueSnapshot> Values
        {
            get { return _values; }
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }
    }
}