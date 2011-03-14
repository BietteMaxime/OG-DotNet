using System.Linq;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotProcessorTests : MarketDataSnapshotManagerTests
    {
        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValues()
        {
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {
                var manageableMarketDataSnapshot = snapshotManager.CreateFromView(ViewName);
                var marketDataSnapshotProcessor = snapshotManager.GetProcessor(ViewName);
                var interpolatedDoublesCurves = marketDataSnapshotProcessor.GetYieldCurves(manageableMarketDataSnapshot);
                Assert.NotEmpty(interpolatedDoublesCurves);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanOverrideYieldCurveValues()
        {
            using (var snapshotManager = Context.MarketDataSnapshotManager)
            {
                var manageableMarketDataSnapshot = snapshotManager.CreateFromView(ViewName);
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves.Values.First();
                foreach (var valueSnapshot in ycSnapshot.Values)
                {
                    valueSnapshot.Value.OverrideValue = 23;
                }

                var marketDataSnapshotProcessor = snapshotManager.GetProcessor(ViewName);
                var interpolatedDoublesCurves = marketDataSnapshotProcessor.GetYieldCurves(manageableMarketDataSnapshot);
                Assert.NotEmpty(interpolatedDoublesCurves);
            }
        }
    }
}