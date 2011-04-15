//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Model.Context;
using OGDotNet.Model.Resources;
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
                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                using (var marketDataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                {
                    var interpolatedDoublesCurves = marketDataSnapshotProcessor.GetYieldCurves();
                    Assert.Equal(interpolatedDoublesCurves.Count, manageableMarketDataSnapshot.YieldCurves.Count);
                }
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetViewOverSnapshot(ViewDefinition viewDefinition)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var remoteClient = Context.CreateUserClient())
            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition))
            {
                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                using (var marketDataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                {
                    var viewOfSnapshot = marketDataSnapshotProcessor.GetViewOfSnapshot(MarketDataSnapshotProcessor.ViewOption.AllSnapshotValues);
                    try
                    {
                        using (var remoteViewClient = Context.ViewProcessor.CreateClient())
                        {
                            remoteViewClient.AttachToViewProcess(viewOfSnapshot.Name, ExecutionOptions.Live);

                            var runOneCycle = remoteViewClient.GetResults(default(CancellationToken)).First();
                            Assert.NotNull(runOneCycle);
                            Assert.NotEmpty(runOneCycle.AllResults);
                        }
                    }
                    finally
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewOfSnapshot.Name);
                    }
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
                foreach (var valueSnapshot in ycSnapshot.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        snapshot.Value.OverrideValue = snapshot.Value.MarketValue * 1.01;
                    }
                }

                var afterCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.NotEmpty(afterCurves);

                var beforeCurve = beforeCurves.First().Value.Item1.Curve;
                var afterCurve = afterCurves.First().Value.Item1.Curve;

                //Curve should change Ys but not x
                Assert.Equal(beforeCurve.XData, afterCurve.XData);

                var diffs = beforeCurve.YData.Zip(afterCurve.YData, DiffProportion).ToList();
                Assert.NotEmpty(diffs.Where(d => d > 0.01).ToList());
            }
        }

        private static double DiffProportion(double a, double b)
        {
            return Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
        }

        [Xunit.Extensions.Fact]
        public void CantGetContradictoryViewOverSnapshot()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                dataSnapshotProcessor.PrepareYieldCurveUpdate(manageableMarketDataSnapshot.YieldCurves.Keys.First()).Execute();

                var marketDataValueSpecification = manageableMarketDataSnapshot.YieldCurves.Values.First().Values.Values.Keys.First();
                var valueSnapshots = manageableMarketDataSnapshot.Values[marketDataValueSpecification];
                var valueSnapsYC = manageableMarketDataSnapshot.YieldCurves.First().Value.Values.Values[marketDataValueSpecification];

                var valueName = valueSnapshots.Keys.First();

                valueSnapshots[valueName].OverrideValue = valueSnapshots[valueName].MarketValue * 1.01;
                valueSnapsYC[valueName].OverrideValue = valueSnapshots[valueName].MarketValue * 0.99;

                Assert.Throws<InvalidOperationException>(() => dataSnapshotProcessor.GetViewOfSnapshot(MarketDataSnapshotProcessor.ViewOption.AllSnapshotValues));
            }
        }
    }
}