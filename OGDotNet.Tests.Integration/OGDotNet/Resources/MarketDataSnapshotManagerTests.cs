using System.Linq;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : TestWithContextBase
    {
        const string ViewName = "Equity Option Test View 1";

        [Xunit.Extensions.Fact]
        public void CanCreateFromView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            var manageableMarketDataSnapshot = snapshotManager.CreateFromView(ViewName);
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

        [Xunit.Extensions.Fact]
        public void CanUpdateFromView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            
            var original = snapshotManager.CreateFromView(ViewName);

            var valueChanged = original.Values.Keys.First();
            original.Values[valueChanged].OverrideValue = 12;


            var manageableMarketDataSnapshot = snapshotManager.UpdateFromView(original, ViewName);

            Assert.Null(manageableMarketDataSnapshot.Name);
            Assert.Null(manageableMarketDataSnapshot.UniqueId);

            Assert.NotEmpty(manageableMarketDataSnapshot.Values);
            foreach (var valueSnapshot in manageableMarketDataSnapshot.Values)
            {
                Assert.Equal(valueSnapshot.Key, valueSnapshot.Value.Security);
                ValueAssertions.AssertSensibleValue(valueSnapshot.Value.MarketValue);
                if (valueSnapshot.Key.Equals(valueChanged))
                {
                    Assert.Equal(12,valueSnapshot.Value.OverrideValue);
                }
                else
                {
                    Assert.Null(valueSnapshot.Value.OverrideValue);
                }
            }

            
        }
    }
}