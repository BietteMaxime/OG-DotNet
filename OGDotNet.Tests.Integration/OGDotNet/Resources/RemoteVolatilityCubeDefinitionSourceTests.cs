//-----------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Tests.Integration.Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteVolatilityCubeDefinitionSourceTests : TestWithContextBase
    {
        [Fact]
        public void CanGetDefaultDefn()
        {
            var defn = Context.VolatilityCubeDefinitionSource.GetDefinition(Currency.USD, "DEFAULT");
            ValueAssertions.AssertSensibleValue(defn);
        }

        [Fact]
        public void CanGetBloombergDefn()
        {
            var defn = Context.VolatilityCubeDefinitionSource.GetDefinition(Currency.USD, "DEFAULT");
            ValueAssertions.AssertSensibleValue(defn);
        }
    }
}
