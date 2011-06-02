//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManagerTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.Context;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanCreateAndRunFromView(ViewDefinition vd)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(vd.Name))
            {
                proc.Snapshot.Name = TestUtils.GetUniqueName();
                Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot));

                try
                {
                    var snapOptions = ExecutionOptions.Snapshot(proc.Snapshot.UniqueId);
                    var withSnapshot = GetFirstResult(snapOptions, vd.Name);

                    var options = ExecutionOptions.SingleCycle;
                    IViewComputationResultModel withoutSnapshot = GetFirstResult(options, vd.Name);

                    var withoutCount = CountResults(withoutSnapshot);
                    var withCount = CountResults(withSnapshot);
                    if (withoutCount != withCount)
                    {
                        Assert.True(false, string.Format("Running snapshot of {0} only had {1}, live had {2}", vd.Name, withCount, withoutCount));
                    }

                    Assert.Equal(withoutCount, withCount);
                }
                finally
                {
                    Context.MarketDataSnapshotMaster.Remove(proc.Snapshot.UniqueId);
                }
            }
        }

        private static IViewComputationResultModel GetFirstResult(IViewExecutionOptions options, string viewName)
        {
            using (var remoteViewClient2 = Context.ViewProcessor.CreateClient())
            {
                return remoteViewClient2.GetResults(viewName, options).First();
            }
        }

        private static int CountResults(IViewComputationResultModel results)
        {
            return results.AllResults.Count();
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanCreateFromView(ViewDefinition viewDefinition)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(viewDefinition))
            {
                var manageableMarketDataSnapshot = proc.Snapshot;
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

                if (viewDefinition.Name != "GlobeOp Bond View Implied")
                {
                    Assert.InRange(manageableMarketDataSnapshot.YieldCurves.Count, ExpectedYieldCurves(viewDefinition), int.MaxValue);
                }
                if (viewDefinition.Name == "Equity Option Test View 1")
                {
                    Assert.Equal(1, manageableMarketDataSnapshot.YieldCurves.Count);
                    var yieldCurveSnapshot =
                        manageableMarketDataSnapshot.YieldCurves[new YieldCurveKey(Currency.USD, "SINGLE")];
                    Assert.NotNull(yieldCurveSnapshot);
                }
                foreach (var curve in manageableMarketDataSnapshot.YieldCurves.Values)
                {
                    AssertSaneValue(curve);
                    Assert.True(curve.Values.Values.Keys.Any(s => ! manageableMarketDataSnapshot.GlobalValues.Values.ContainsKey(s))); //LAP-37
                }
            }
        }

        private static int ExpectedYieldCurves(ViewDefinition viewDefinition)
        {
            var yieldCurvesPerConfiguration = viewDefinition.CalculationConfigurationsByName.Select(
                c => c.Value.SpecificRequirements.Where(r => r.ValueName.Contains("YieldCurve")).Count());
            return
                yieldCurvesPerConfiguration.Distinct().Max();
        }

        [Xunit.Extensions.Fact]
        public void CanUpdateFromView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(RemoteViewClientBatchTests.ViewName))
            {
                var updated = proc.Snapshot;

                var targetChanged = updated.Values.Keys.First();
                var valueChanged = updated.Values[targetChanged].Keys.First();

                updated.Values[targetChanged][valueChanged].OverrideValue = 12;

                bool seenUpdate = false;
                foreach (var value in updated.GlobalValues.Values.SelectMany(v => v.Value.Values))
                {
                    value.PropertyChanged += delegate { seenUpdate = true; };
                }

                var action = proc.PrepareUpdate();
                Assert.Empty(action.Warnings);
                Assert.False(seenUpdate);
                action.Execute(updated);
                Assert.True(seenUpdate);

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

        [Xunit.Extensions.Fact]
        public void UpdateCanBeReverseApplied()
        {
            WeakReference z;
            UpdateAction<ManageableMarketDataSnapshot> x;
            WeakReference y;
            ReverseApply(out x, out y, out z);
        }

        [Xunit.Extensions.Fact]
        public void UpdateDoesntReferenceSnapshot()
        {
            WeakReference z;
            UpdateAction<ManageableMarketDataSnapshot> x;
            WeakReference y;
            ReverseApply(out x, out y, out z);
            GC.Collect();
            Assert.False(z.IsAlive);
            Assert.False(y.IsAlive);
        }

        private static void ReverseApply(out UpdateAction<ManageableMarketDataSnapshot> fwdAction, out WeakReference beforeRef, out WeakReference afterRef)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(RemoteViewClientBatchTests.ViewName))
            {
                using (var newProc = snapshotManager.CreateFromViewDefinition(RemoteViewClientBatchTests.ViewName))
                {
                    //TODO more strict
                    var testedMds = proc.Snapshot.GlobalValues.Values.First().Key;
                    var valueSnapshots = newProc.Snapshot.GlobalValues.Values[testedMds];
                    valueSnapshots.Remove(valueSnapshots.Keys.First());

                    UpdateAction<ManageableMarketDataSnapshot> fwd = proc.Snapshot.PrepareUpdateFrom(newProc.Snapshot);
                    UpdateAction<ManageableMarketDataSnapshot> bwd = newProc.Snapshot.PrepareUpdateFrom(proc.Snapshot);

                    var pre = proc.Snapshot.GlobalValues.Values[testedMds].ToDictionary(k => k.Key, k => k.Value.MarketValue);

                    fwd.Execute(proc.Snapshot);
                    bwd.Execute(proc.Snapshot);

                    var post = proc.Snapshot.GlobalValues.Values[testedMds].ToDictionary(k => k.Key, k => k.Value.MarketValue);

                    Assert.True(pre.Keys.Concat(post.Keys).All(k => pre[k] == post[k]));
                    
                    bwd.Execute(newProc.Snapshot);
                    fwd.Execute(newProc.Snapshot);
                    
                    beforeRef = new WeakReference(proc.Snapshot);
                    fwdAction = fwd;
                    afterRef = new WeakReference(newProc.Snapshot);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void NoTicksViewDoesntTick()
        {
            Assert.False(CausesTick(ExecutionOptions.Snapshot, p => { }));
        }

        [Xunit.Extensions.Fact]
        public void UpdatingSnapshotTicksView()
        {
            Assert.True(CausesTick(delegate(MarketDataSnapshotProcessor proc)
                                       {
                                           var action = proc.PrepareUpdate();
                                           action.Execute(proc.Snapshot);

                                           Context.MarketDataSnapshotMaster.Update((new MarketDataSnapshotDocument(proc.Snapshot.UniqueId, proc.Snapshot)));
                                       }));
        }

        [Xunit.Extensions.Fact]
        public void AddingSnapshotDoesntTickView()
        {
            Assert.False(CausesTick(delegate(MarketDataSnapshotProcessor proc)
            {
                var action = proc.PrepareUpdate();
                action.Execute(proc.Snapshot);

                proc.Snapshot.UniqueId = null;
                Context.MarketDataSnapshotMaster.Add((new MarketDataSnapshotDocument(null, proc.Snapshot)));
            }));
        }

        [Xunit.Extensions.Fact]
        public void UpdatingSnapshotVersionedDoesntTickView()
        {
            Assert.False(CausesTick(
                ExecutionOptions.Snapshot,
                delegate(MarketDataSnapshotProcessor proc)
                {
                    var action = proc.PrepareUpdate();
                    action.Execute(proc.Snapshot);

                    proc.Snapshot.UniqueId = null;
                    Context.MarketDataSnapshotMaster.Add((new MarketDataSnapshotDocument(null, proc.Snapshot)));
                }));
        }

        private static bool CausesTick(Action<MarketDataSnapshotProcessor> action)
        {
            return CausesTick(u => ExecutionOptions.Snapshot(u.ToLatest()), action);
        }

        private static bool CausesTick(Func<UniqueIdentifier, IViewExecutionOptions> optionsFactory, Action<MarketDataSnapshotProcessor> action)
        {
            bool ret = false;
            var snapshotManager = Context.MarketDataSnapshotManager;
            WithNoTicksView(delegate(string viewName)
            {
                using (var proc = snapshotManager.CreateFromViewDefinition(viewName))
                {
                    proc.Snapshot.Name = TestUtils.GetUniqueName();
                    Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot));

                    var options = optionsFactory(proc.Snapshot.UniqueId);

                    using (var remoteViewClient = Context.ViewProcessor.CreateClient())
                    {
                        var viewComputationResultModels = remoteViewClient.GetResults(viewName, options);
                        using (var enumerator = viewComputationResultModels.GetEnumerator())
                        {
                            Assert.True(enumerator.MoveNext());

                            var task = new Task<bool>(enumerator.MoveNext);
                            task.Start();

                            Assert.False(task.Wait(TimeSpan.FromSeconds(10)));

                            action(proc);
                            ret = task.Wait(TimeSpan.FromSeconds(10));
                            if (! ret)
                            {
                                ((Task) task).ContinueWith(t => { var ignore = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                            }
                        }
                    }
                }
            });
            return ret;
        }

        private static void WithNoTicksView(Action<string> action)
        {
            using (var remoteClient = Context.CreateUserClient())
            {
                var viewDefinition =
                    Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(RemoteViewClientBatchTests.ViewName);

                viewDefinition.Name = string.Format("{0}-NoTicks-{1}", viewDefinition.Name, Guid.NewGuid());
                
                viewDefinition.MinFullCalcPeriod = null;
                viewDefinition.MinDeltaCalcPeriod = null;
                viewDefinition.MaxFullCalcPeriod = TimeSpan.FromDays(1);
                viewDefinition.MaxDeltaCalcPeriod = TimeSpan.FromDays(1);
                try
                {
                    remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                    action(viewDefinition.Name);
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                }
            }
        }

        private static void AssertSaneValue(ManageableYieldCurveSnapshot yieldCurveSnapshot)
        {
            Assert.NotNull(yieldCurveSnapshot);
            Assert.InRange(yieldCurveSnapshot.Values.Values.Count(), 2, 200);

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