using System.Linq;
using Castle.Core;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : TestWithContextBase
    {
        protected const string ViewName = "Equity Option Test View 1";

        [Xunit.Extensions.Fact]
        public void CanCreateFromView()
        {
            
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {
                var manageableMarketDataSnapshot = snapshotManager.CreateFromView(ViewName);
                Assert.Null(manageableMarketDataSnapshot.Name);
                Assert.Null(manageableMarketDataSnapshot.UniqueId);

                Assert.NotEmpty(manageableMarketDataSnapshot.Values);
                foreach (var valueSnapshot in manageableMarketDataSnapshot.Values)
                {
                    Assert.Equal(valueSnapshot.Key, valueSnapshot.Value.Security);
                    ValueAssertions.AssertSensibleValue(valueSnapshot.Value.MarketValue);
                    Assert.Null(valueSnapshot.Value.OverrideValue);
                }
                
                Assert.Equal(1, manageableMarketDataSnapshot.YieldCurves.Count);
                var yieldCurveSnapshot = manageableMarketDataSnapshot.YieldCurves[new Pair<string, Currency>("Default", Currency.Create("USD"))];
                AssertSaneValue(yieldCurveSnapshot);
            }
        }

        

        [Xunit.Extensions.Fact]
        public void CanUpdateFromView()
        {
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {

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
                        Assert.Equal(12, valueSnapshot.Value.OverrideValue);
                    }
                    else
                    {
                        Assert.Null(valueSnapshot.Value.OverrideValue);
                    }
                }

            }
        }

        private static void AssertSaneValue(YieldCurveSnapshot yieldCurveSnapshot)
        {
            Assert.NotNull(yieldCurveSnapshot);
            Assert.InRange(yieldCurveSnapshot.Values.Count, 2,200);
            foreach (var valueSnapshot in yieldCurveSnapshot.Values)
            {
                Assert.NotNull(valueSnapshot.Key);
                ValueAssertions.AssertSensibleValue(valueSnapshot.Value.MarketValue);
                Assert.Null(valueSnapshot.Value.OverrideValue);
            }
        }

    }
}