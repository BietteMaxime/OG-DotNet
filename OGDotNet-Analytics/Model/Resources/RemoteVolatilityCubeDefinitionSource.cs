using System;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;
namespace OGDotNet.Model.Resources
{
    public class RemoteVolatilityCubeDefinitionSource
    {
        private readonly RestTarget _restTarget;

        public RemoteVolatilityCubeDefinitionSource(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public VolatilityCubeDefinition GetDefinition(Currency currency, String name)
        {
            return _restTarget.Resolve(currency.ISOCode).Resolve(name).Get<VolatilityCubeDefinition>("definition");
        }
    }
}
