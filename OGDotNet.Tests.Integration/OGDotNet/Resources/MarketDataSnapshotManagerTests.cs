using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanCreateFromView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            var manageableMarketDataSnapshot = snapshotManager.CreateFromView("Equity Option Test View 1");
            Assert.Null(manageableMarketDataSnapshot.Name);
            Assert.Null(manageableMarketDataSnapshot.UniqueId);

            Assert.NotEmpty(manageableMarketDataSnapshot.Values);
            foreach (var valueSnapshot in manageableMarketDataSnapshot.Values)
            {
                Assert.Equal(valueSnapshot.Key,valueSnapshot.Value.Security);
                ValueAssertions.AssertSensibleValue(valueSnapshot.Value.MarketValue);
                Assert.Null(valueSnapshot.Value.OverrideValue);
            }
        }
    }
}