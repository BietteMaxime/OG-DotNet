using OGDotNet.Mappedtypes.Core.Common;
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
