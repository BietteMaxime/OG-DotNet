//-----------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Util.Money;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteVolatilityCubeDefinitionSourceTests : TestWithContextBase
    {
        [Xunit.Extensions.Theory]
        [InlineData("DEFAULT")]
        [InlineData("BLOOMBERG")]
        public void CanGetDefaultDefn(string cubeName)
        {
            var defn = Context.VolatilityCubeDefinitionSource.GetDefinition(Currency.USD, cubeName);
            ValueAssertions.AssertSensibleValue(defn);
        }
    }
}
