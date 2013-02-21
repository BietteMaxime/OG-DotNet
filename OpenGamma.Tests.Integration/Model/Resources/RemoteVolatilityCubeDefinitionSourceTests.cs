// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteVolatilityCubeDefinitionSourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Util.Money;

using Xunit.Extensions;

namespace OpenGamma.Model.Resources
{
    public class RemoteVolatilityCubeDefinitionSourceTests : RemoteEngineContextTestBase
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
