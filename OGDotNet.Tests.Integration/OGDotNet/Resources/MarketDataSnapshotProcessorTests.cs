//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotProcessorTests : ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetYieldCurveValues(ViewDefinition viewDefinition)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition))
            {
                var valuedCurves = dataSnapshotProcessor.GetYieldCurves();
                var snappedCurves = dataSnapshotProcessor.Snapshot.YieldCurves;
                var snappedCurveCount = snappedCurves.Count;
                var valuedCurveCount = valuedCurves.Count;

                if (snappedCurveCount != valuedCurveCount)
                {
                    Assert.False(true, string.Format(
                        "Only found {0} curves, snapshotted {1} for view {2}.  missing curves {3}",
                        valuedCurveCount, snappedCurveCount, viewDefinition.Name, string.Join(",", snappedCurves.Where(s => !valuedCurves.Any(c => c.Key.Equals(s.Key))).Select(s => s.Key))));    
                }
            }
        }

        private const string ViewDefinitionName = "Equity Option Test View 1";

        [Xunit.Extensions.Fact]
        public void CanOverrideYieldCurveValuesEqView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var beforeCurves = dataSnapshotProcessor.GetYieldCurves();

                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves.Values.First();

                ValueSnapshot value = ycSnapshot.Values.Values.First().Value.First().Value;
                value.OverrideValue = value.MarketValue * 1.01;

                var afterCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.NotEmpty(afterCurves);

                var beforeCurve = beforeCurves.First().Value.Item1.Curve;
                var afterCurve = afterCurves.First().Value.Item1.Curve;

                //Curve should change Ys but not x
                Assert.Equal(beforeCurve.XData, afterCurve.XData);

                var diffs = beforeCurve.YData.Zip(afterCurve.YData, DiffProportion).ToList();
                Assert.NotEmpty(diffs.Where(d => d > 0.001).ToList());
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetLiveDataStream(ViewDefinition viewDefinition)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition))
            {
                using (var art = new AutoResetEvent(false))
                {
                    using (LiveDataStream liveDataStream = dataSnapshotProcessor.GetLiveDataStream())
                    {
                        liveDataStream.PropertyChanged += delegate
                                                              {
                                                                  art.Set();
                                                              };

                        TimeSpan timeout = TimeSpan.FromSeconds(30);
                        Assert.True(art.WaitOne(timeout));

                        foreach (var set in dataSnapshotProcessor.Snapshot.GlobalValues.Values)
                        {
                            foreach (var entry in set.Value)
                            {
                                Assert.NotNull(liveDataStream[set.Key, entry.Key]);   
                            }
                        }
                        Assert.True(art.WaitOne(timeout));
                    }
                }
            }
        }

        private static double DiffProportion(double a, double b)
        {
            return Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
        }
    }
}