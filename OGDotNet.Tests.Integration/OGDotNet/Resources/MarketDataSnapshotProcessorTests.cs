using System.Linq;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotProcessorTests : ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void CanGetYieldCurveValues(RemoteView view)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromView(view))
            {
                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                using (var marketDataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                {
                    var interpolatedDoublesCurves = marketDataSnapshotProcessor.GetYieldCurves();
                    Assert.Equal(interpolatedDoublesCurves.Count, manageableMarketDataSnapshot.YieldCurves.Count);
                }
            }
        }

        private const string ViewName = "Equity Option Test View 1";




        [Xunit.Extensions.Fact]
        public void CanOverrideYieldCurveValuesEqView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromView(ViewName))
            {
                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves.Values.First();
                foreach (var valueSnapshot in ycSnapshot.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        snapshot.Value.OverrideValue = 23;
                    }
                }

                var interpolatedDoublesCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.NotEmpty(interpolatedDoublesCurves);
                //TODO check that the curve changes
            }
        }
    }
}