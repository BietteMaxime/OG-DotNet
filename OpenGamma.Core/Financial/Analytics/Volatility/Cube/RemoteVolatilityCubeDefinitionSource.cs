// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Model;
using OpenGamma.Util.Money;

namespace OpenGamma.Financial.Analytics.Volatility.Cube
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
            return _restTarget.Resolve("definitions", "searchSingle").WithParam("currency", currency.ISOCode).WithParam("name", name).Get<VolatilityCubeDefinition>();
        }
    }
}
