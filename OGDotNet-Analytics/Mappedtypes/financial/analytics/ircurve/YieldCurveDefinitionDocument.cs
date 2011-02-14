using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinitionDocument
    {
        public YieldCurveDefinition YieldCurveDefinition { get; set; }

        public UniqueIdentifier UniqueId { get; set; }
    }
}