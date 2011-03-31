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
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {
                var manageableMarketDataSnapshot = snapshotManager.CreateFromView(view);
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
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {
                var manageableMarketDataSnapshot = snapshotManager.CreateFromView(ViewName);
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves.Values.First();
                foreach (var valueSnapshot in ycSnapshot.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        snapshot.Value.OverrideValue = 23;
                    }
                }

                using (var marketDataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                {
                    var interpolatedDoublesCurves = marketDataSnapshotProcessor.GetYieldCurves();
                    Assert.NotEmpty(interpolatedDoublesCurves);
                    //TODO check that the curve changes
                }
            }
        }
    }
}