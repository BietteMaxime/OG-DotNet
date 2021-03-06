﻿//-----------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Cube;
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
            return _restTarget.Resolve("definitions", "searchSingle").WithParam("currency", currency.ISOCode).WithParam("name", name).Get<VolatilityCubeDefinition>();
        }
    }
}
