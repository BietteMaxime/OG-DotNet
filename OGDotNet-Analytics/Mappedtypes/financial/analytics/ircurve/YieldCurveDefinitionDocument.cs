using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinitionDocument
    {
        public YieldCurveDefinition Definition { get; set; }

        public UniqueIdentifier UniqueId { get; set; }
    }
}