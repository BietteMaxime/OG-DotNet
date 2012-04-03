//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Analytics.Math.Curve;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Model.Context;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
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
                GetAndCheckYieldCurves(dataSnapshotProcessor);
            }
        }

        private const string ViewDefinitionName = "Equity Option Test View 1";

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValuesRepeatedly()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                for (int i = 0; i < 3; i++)
                {
                    GetAndCheckYieldCurves(dataSnapshotProcessor);
                }
            }
        }

        private static void GetAndCheckYieldCurves(MarketDataSnapshotProcessor dataSnapshotProcessor)
        {
            var valuedCurves = dataSnapshotProcessor.GetYieldCurves();

            CheckYieldCurves(dataSnapshotProcessor, valuedCurves);
        }

        private static void CheckYieldCurves(MarketDataSnapshotProcessor dataSnapshotProcessor, Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> valuedCurves)
        {
            var unexpectedCurves = valuedCurves.Keys.Except(dataSnapshotProcessor.Snapshot.YieldCurves.Keys);
            if (unexpectedCurves.Any())
            {
                Assert.False(true, string.Format(
                    "Found {0} curves which weren't in snapshot",
                    string.Join(",", unexpectedCurves)));
            }
        }

        [Xunit.Extensions.Fact(Skip = "[DOTNET-36] This doesn't pass on the build server for some reason")]
        public void CanOverrideYieldCurveValuesEqView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var beforeCurves = dataSnapshotProcessor.GetYieldCurves();
                var beforeCurve = beforeCurves.First().Value.Item1.Curve;

                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves.Values.First();

                ValueSnapshot value = ycSnapshot.Values.Values.First().Value.First().Value;
                value.OverrideValue = value.MarketValue * 1.01;

                var afterCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.NotEmpty(afterCurves);
                var afterCurve = afterCurves.First().Value.Item1.Curve;

                //Curve should change Ys but not x
                Assert.Equal(beforeCurve.XData, afterCurve.XData);

                var diffs = beforeCurve.YData.Zip(afterCurve.YData, DiffProportion).ToList();
                Assert.NotEmpty(diffs.Where(d => d > 0.001).ToList());
            }
        }

        [Xunit.Extensions.Fact]
        public void CanCancelGettingYieldCurves()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                using (var cts = new CancellationTokenSource())
                {
                    var yieldCurves = dataSnapshotProcessor.GetYieldCurves(cts.Token);
                    CheckYieldCurves(dataSnapshotProcessor, yieldCurves);

                    cts.Cancel();
                    Assert.Throws<OperationCanceledException>(() => dataSnapshotProcessor.GetYieldCurves(cts.Token));
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanCancelGettingYieldCurvesAfterStart()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                using (var cts = new CancellationTokenSource())
                {
                    TimeSpan when = TimeSpan.FromSeconds(0.2);
                    ThreadPool.QueueUserWorkItem(delegate
                                                     {
                                                         Thread.Sleep(when);
                                                         cts.Cancel();
                                                     });
                    Assert.Throws<OperationCanceledException>(() => dataSnapshotProcessor.GetYieldCurves(cts.Token));
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanDoStupidOverride()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var beforeCurves = dataSnapshotProcessor.GetYieldCurves();

                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                var key = beforeCurves.Where(c => c.Value != null).First().Key;
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves[key];
                foreach (var value in ycSnapshot.Values.Values)
                {
                    foreach (var vs in value.Value.Values)
                    {
                        vs.OverrideValue = vs.MarketValue * -1000;        
                    }
                }

                var afterCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.Equal(beforeCurves.Count, afterCurves.Count);
                Assert.Equal(beforeCurves.Where(c => c.Value != null).Count() - 1, afterCurves.Where(c => c.Value != null).Count());
            }
        }

        [Xunit.Extensions.Fact]
        public void GettingYieldCurveValuesIsQuick()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var beforeCurves = dataSnapshotProcessor.GetYieldCurves();
                YieldCurveKey curveKey = beforeCurves.First(k => k.Value != null).Key;
                var beforeCurve = beforeCurves[curveKey].Item1.Curve;

                var manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                var ycSnapshot = manageableMarketDataSnapshot.YieldCurves[curveKey];

                const double f = 1.001;

                foreach (var value1 in ycSnapshot.Values.Values)
                {
                    foreach (var valueSnapshot in value1.Value.Values)
                    {
                        valueSnapshot.OverrideValue = valueSnapshot.MarketValue * f;
                    }
                }

                var afterCurves = dataSnapshotProcessor.GetYieldCurves();
                Assert.NotEmpty(afterCurves);

                Assert.Empty(beforeCurves.Keys.Except(afterCurves.Keys));
                
                var afterCurve = afterCurves[curveKey].Item1.Curve;

                //Curve should change Ys but not x
                Assert.Equal(beforeCurve.XData, afterCurve.XData);

                var diffs = beforeCurve.YData.Zip(afterCurve.YData, DiffProportion).ToList();
                Assert.NotEmpty(diffs.Where(d => d > 0.001).ToList());

                foreach (var value1 in ycSnapshot.Values.Values)
                {
                    foreach (var valueSnapshot in value1.Value.Values)
                    {
                        valueSnapshot.OverrideValue = null;
                    }
                }

                Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> timedCurves = null;

                TimeSpan time = Time(() => timedCurves = dataSnapshotProcessor.GetYieldCurves());
                Assert.InRange(time, TimeSpan.Zero, TimeSpan.FromSeconds(3)); // TODO faster
                Console.Out.WriteLine(time);

                var diffs2 = beforeCurves[curveKey].Item1.Curve.YData.Zip(timedCurves[curveKey].Item1.Curve.YData, DiffProportion).ToList();
                Assert.Empty(diffs2.Where(d => d > 0.001).ToList());

                //TODO check nodal curves
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanUpdateFromLiveDataStream(ViewDefinition viewDefinition)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition))
            {
                UpdateAction<ManageableMarketDataSnapshot> prepareUpdate = dataSnapshotProcessor.PrepareUpdate();
                var before = GetCount(dataSnapshotProcessor);
                prepareUpdate.Execute(dataSnapshotProcessor.Snapshot);

                var after = GetCount(dataSnapshotProcessor);
                Assert.Equal(before, after);
            }
        }

        [Xunit.Extensions.Fact]
        public void UpdatingFromLiveDataStreamIsFast()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(ViewDefinitionName))
            {
                var fromStream = Time(() => dataSnapshotProcessor.PrepareUpdate());
                Assert.InRange(fromStream, TimeSpan.Zero, TimeSpan.FromSeconds(4));
                //TODO check that we have actually updated
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValuesAfterRemovingView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            var viewDefinition = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(ViewDefinitionName);
            using (var remoteClient = Context.CreateFinancialClient())
            {
                SetTemporaryName(viewDefinition);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                try
                {
                    using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition.Name))
                    {
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                }
                finally
                {
                    if (Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewDefinition.Name) != null)
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    }
                }
            }
        }

        private static void SetTemporaryName(ViewDefinition viewDefinition)
        {
            viewDefinition.Name = string.Format("{0}-RoundTripped-{1}", viewDefinition.Name, TestUtils.GetUniqueName());
            viewDefinition.UniqueID = null;
        }

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValuesAfterRemovingViewAndReconnecting() //LAP-66
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            var viewDefinition = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(ViewDefinitionName);
            using (var remoteClient = Context.CreateFinancialClient())
            {
                SetTemporaryName(viewDefinition);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                try
                {
                    ManageableMarketDataSnapshot manageableMarketDataSnapshot;
                    using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition.Name))
                    {
                        manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    manageableMarketDataSnapshot.BasisViewName = ViewDefinitionName;
                    using (var dataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                    {
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                }
                finally
                {
                    if (Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewDefinition.Name) != null)
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    }
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValuesAfterChangingName() //LAP-66
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            var viewDefinition = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(ViewDefinitionName);
            using (var remoteClient = Context.CreateFinancialClient())
            {
                SetTemporaryName(viewDefinition);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                try
                {
                    ManageableMarketDataSnapshot manageableMarketDataSnapshot;
                    using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition.Name))
                    {
                        manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    using (var dataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                    {
                        manageableMarketDataSnapshot.BasisViewName = ViewDefinitionName;
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                }
                finally
                {
                    if (Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewDefinition.Name) != null)
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    }
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveValuesAfterChangingNameAndWaiting() //LAP-66
        {
            var snapshotManager = Context.MarketDataSnapshotManager;

            var viewDefinition = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(ViewDefinitionName);
            using (var remoteClient = Context.CreateFinancialClient())
            {
                SetTemporaryName(viewDefinition);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                try
                {
                    ManageableMarketDataSnapshot manageableMarketDataSnapshot;
                    using (var dataSnapshotProcessor = snapshotManager.CreateFromViewDefinition(viewDefinition.Name))
                    {
                        manageableMarketDataSnapshot = dataSnapshotProcessor.Snapshot;
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    using (var dataSnapshotProcessor = snapshotManager.GetProcessor(manageableMarketDataSnapshot))
                    {
                        Assert.Throws<OpenGammaException>(() => GetAndCheckYieldCurves(dataSnapshotProcessor));
                        manageableMarketDataSnapshot.BasisViewName = ViewDefinitionName;
                        GetAndCheckYieldCurves(dataSnapshotProcessor);
                    }
                }
                finally
                {
                    if (Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewDefinition.Name) != null)
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    }
                }
            }
        }

        private static TimeSpan Time(Action act)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            act();
            s.Stop();
            return s.Elapsed;
        }

        private static Tuple<int, int> GetCount(MarketDataSnapshotProcessor dataSnapshotProcessor)
        {
            int count = dataSnapshotProcessor.Snapshot.GlobalValues.Values.Count;
            int ycCount = dataSnapshotProcessor.Snapshot.YieldCurves.Any() ? dataSnapshotProcessor.Snapshot.YieldCurves.First().Value.Values.Values.Count : 0;
            return Tuple.Create(count, ycCount);
        }

        private static double DiffProportion(double a, double b)
        {
            return Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
        }
    }
}