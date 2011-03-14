using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class ValueSnapshot
    {
        public UniqueIdentifier Security { get; set; }

        public double MarketValue { get; set; }

        public double? OverrideValue { get; set; }
    }
}
