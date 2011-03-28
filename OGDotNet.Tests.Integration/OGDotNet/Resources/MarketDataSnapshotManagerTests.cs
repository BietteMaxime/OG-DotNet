using System.Linq;
using Castle.Core;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : TestWithContextBase
    {
        protected const string ViewName = "Equity Option Test View 1";
        //protected const string ViewName = "Swap Test View";

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
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                        Assert.Null(snapshot.Value.OverrideValue);    
                    }
                    
                }
                
                Assert.Equal(1, manageableMarketDataSnapshot.YieldCurves.Count);
                var yieldCurveSnapshot = manageableMarketDataSnapshot.YieldCurves[new YieldCurveKey(Currency.Create("USD"), "Default")];
                AssertSaneValue(yieldCurveSnapshot);
            }
        }


        [Xunit.Extensions.Fact]
        public void CanUpdateFromView()
        {
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {

                var updated = snapshotManager.CreateFromView(ViewName);

                var targetChanged = updated.Values.Keys.First();
                var valueChanged = updated.Values[targetChanged].Keys.First();

                updated.Values[targetChanged][valueChanged].OverrideValue = 12;


                snapshotManager.UpdateFromView(updated, ViewName);

				Assert.Null(updated.Name);
                Assert.Null(updated.UniqueId);

                Assert.NotEmpty(updated.Values);

                foreach (var valueSnapshot in updated.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                        if (targetChanged == valueSnapshot.Key && valueChanged == snapshot.Key)
                        {
                            Assert.Equal(12, snapshot.Value.OverrideValue);
                        }
                        else
                        {
                            Assert.Null(snapshot.Value.OverrideValue);                            
                        }
                    }

                }
                

            }
        }

        private static void AssertSaneValue(ManageableYieldCurveSnapshot yieldCurveSnapshot)
        {
            Assert.NotNull(yieldCurveSnapshot);
            Assert.InRange(yieldCurveSnapshot.Values.Values.Count(), 2,200);

            foreach (var valueSnapshot in yieldCurveSnapshot.Values)
            {
                foreach (var snapshot in valueSnapshot.Value)
                {
                    ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                    Assert.Null(snapshot.Value.OverrideValue);
                }

            }
        }

    }
}