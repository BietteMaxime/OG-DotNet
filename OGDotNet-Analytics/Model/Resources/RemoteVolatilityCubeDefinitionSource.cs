//-----------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;
namespace OGDotNet.Model.Resources
{
    public class RemoteVolatilityCubeDefinitionSource
    {
        private readonly RestTarget _restTarget;

        public RemoteVolatilityCubeDefinitionSource(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public VolatilityCubeDefinition GetDefinition(Currency currency, string name)
        {
            return _restTarget.Resolve(currency.ISOCode).Resolve(name).Get<VolatilityCubeDefinition>("definition");
        }
    }
}
