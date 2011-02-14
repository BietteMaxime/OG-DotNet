using System;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    [Serializable]
    public class YieldCurveDefinitionDocument
    {
        public UniqueIdentifier UniqueId;

        public YieldCurveDefinition YieldCurveDefinition;
    }
}