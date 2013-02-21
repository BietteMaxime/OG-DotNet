// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManagerTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Financial.Analytics.Volatility.Cube;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot;
using OpenGamma.MarketDataSnapshot.Impl;
using OpenGamma.Master.MarketDataSnapshot;
using OpenGamma.Model.Context;
using OpenGamma.Model.Context.MarketDataSnapshot;
using OpenGamma.Xunit.Extensions;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class MarketDataSnapshotManagerTests : ViewTestBase
    {
        [Xunit.Extensions.Fact(Skip = "No obviosuly broken views")]
        public void CantCreateFromBrokenView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            Assert.Throws<OpenGammaException>(() => snapshotManager.CreateFromViewDefinition("OvernightBatchTestView"));
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanCreateAndRunFromView(ViewDefinition vd)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(vd.Name))
            {
                proc.Snapshot.Name = TestUtils.GetUniqueName();
                var uid = Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot)).UniqueId;

                try
                {
                    var snapOptions = ExecutionOptions.Snapshot(proc.Snapshot.UniqueId);
                    var withSnapshot = GetFirstResult(snapOptions, vd.UniqueId);

                    var options = ExecutionOptions.SingleCycle;
                    IViewComputationResultModel withoutSnapshot = GetFirstResult(options, vd.UniqueId);

                    var withoutCount = CountResults(withoutSnapshot);
                    var withCount = CountResults(withSnapshot);
                    if (withoutCount != withCount)
                    {
                        var withSpecs = new HashSet<ValueSpecification>(withSnapshot.AllResults.Select(r => r.ComputedValue.Specification));
                        var withoutSpecs = new HashSet<ValueSpecification>(withoutSnapshot.AllResults.Select(r => r.ComputedValue.Specification));
                        withoutSpecs.SymmetricExceptWith(withSpecs);
                        Assert.True(false, string.Format("Running snapshot of {0} only had {1}, live had {2}", vd.Name, withCount, withoutCount));
                    }

                    Assert.Equal(withoutCount, withCount);
                }
                finally
                {
                    Context.MarketDataSnapshotMaster.Remove(uid);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanCombineSnapshots()
        {
            ViewDefinition vd = GetViewDefinition(@"Demo Equity Option Test View");
            var snapshotManager = Context.MarketDataSnapshotManager;
            Tuple<ManageableMarketDataSnapshot, ManageableMarketDataSnapshot> snaps;
            using (var proc = snapshotManager.CreateFromViewDefinition(vd))
            {
                var snapshot = proc.Snapshot;
                snaps = Halve(snapshot);

                snaps.Item1.Name = TestUtils.GetUniqueName();
                snaps.Item2.Name = TestUtils.GetUniqueName();

                Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, snaps.Item1));
                Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, snaps.Item2));
            }

            try
            {
                var snapOptions = ExecutionOptions.GetSingleCycle(new CombinedMarketDataSpecification(new UserMarketDataSpecification(snaps.Item2.UniqueId), new UserMarketDataSpecification(snaps.Item1.UniqueId)));
                var withSnapshot = GetFirstResult(snapOptions, vd.UniqueId);

                var options = ExecutionOptions.SingleCycle;
                IViewComputationResultModel withoutSnapshot = GetFirstResult(options, vd.UniqueId);

                var withoutCount = CountResults(withoutSnapshot);
                var withCount = CountResults(withSnapshot);
                if (withoutCount != withCount)
                {
                    var withSpecs = new HashSet<ValueSpecification>(withSnapshot.AllResults.Select(r => r.ComputedValue.Specification));
                    var withoutSpecs = new HashSet<ValueSpecification>(withoutSnapshot.AllResults.Select(r => r.ComputedValue.Specification));
                    withoutSpecs.SymmetricExceptWith(withSpecs);
                    Assert.True(false, string.Format("Running snapshot of {0} only had {1}, live had {2}", vd.Name, withCount, withoutCount));
                }

                Assert.Equal(withoutCount, withCount);
            }
            finally
            {
                Context.MarketDataSnapshotMaster.Remove(snaps.Item1.UniqueId);
                Context.MarketDataSnapshotMaster.Remove(snaps.Item2.UniqueId);
            }
        }

        private static Tuple<ManageableMarketDataSnapshot, ManageableMarketDataSnapshot> Halve(ManageableMarketDataSnapshot snapshot)
        {
            var structured = new ManageableMarketDataSnapshot(snapshot.BasisViewName, new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>()), snapshot.YieldCurves, 
                                                                                snapshot.VolatilityCubes, snapshot.VolatilitySurfaces);
            var unstructured = new ManageableMarketDataSnapshot(snapshot.BasisViewName, snapshot.GlobalValues, new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>(), 
                                                                                new Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>(), new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>());
            return Tuple.Create(structured, unstructured);
        }

        // TOD: Test that we haven't included market data globally which should only be in the structured objects
        private static IViewComputationResultModel GetFirstResult(IViewExecutionOptions options, UniqueId viewDefinitionId)
        {
            using (var remoteViewClient2 = Context.ViewProcessor.CreateViewClient())
            {
                return remoteViewClient2.GetResults(viewDefinitionId, options).First();
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

                Assert.True(manageableMarketDataSnapshot.Values.Any() || manageableMarketDataSnapshot.YieldCurves.Any(c => c.Value.Values.Values.Any()) || manageableMarketDataSnapshot.VolatilityCubes.Any(v => v.Value.Values.Any()));
                foreach (var valueSnapshot in manageableMarketDataSnapshot.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                        Assert.Null(snapshot.Value.OverrideValue);
                    }
                }

                if (viewDefinition.Name != "GlobeOp Bond View Implied" /* LAP-38 */ && viewDefinition.Name != "Simple Cash Test View 2" /* LAP-82 */)
                {
                    Assert.InRange(manageableMarketDataSnapshot.YieldCurves.Count, ExpectedYieldCurves(viewDefinition), int.MaxValue);
                }

                if (viewDefinition.Name == "Equity Option Test View 1")
                {
                    Assert.Equal(2, manageableMarketDataSnapshot.YieldCurves.Count);
                    var yieldCurveSnapshot = manageableMarketDataSnapshot.YieldCurves.First();
                    Assert.NotNull(yieldCurveSnapshot);
                    Assert.NotEmpty(yieldCurveSnapshot.Value.Values.Values);
                }

                foreach (var curve in manageableMarketDataSnapshot.YieldCurves.Values)
                {
                    AssertSaneValue(curve);
                    Assert.True(curve.Values.Values.Keys.Any(s => !manageableMarketDataSnapshot.GlobalValues.Values.ContainsKey(s))); // LAP-37
                }

                foreach (var cube in manageableMarketDataSnapshot.VolatilityCubes.Values)
                {
                    Assert.True(cube.Values.Any(v => v.Value.MarketValue == null));
                }
            }
        }

        private static int ExpectedYieldCurves(ViewDefinition viewDefinition)
        {
            var yieldCurvesPerConfiguration = viewDefinition.CalculationConfigurationsByName.Select(
                c => c.Value.SpecificRequirements.Where(r => r.ValueName.Contains("YieldCurve") && r.ValueName != ValueRequirementNames.YieldCurveJacobian).Count());
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

                var beforeCount = updated.Values.Count;

                var targetChanged = updated.Values.Keys.First();
                var valueChanged = updated.Values[targetChanged].Keys.First();

                updated.Values[targetChanged][valueChanged].OverrideValue = 12;

                long updatesSeen = 0;
                long yieldCurveUpdatesSeen = 0;
                foreach (var value in updated.GlobalValues.Values.SelectMany(v => v.Value.Values))
                {
                    value.PropertyChanged += delegate { Interlocked.Increment(ref updatesSeen); };
                }

                foreach (var value in updated.YieldCurves.Values.SelectMany(v => v.Values.Values.SelectMany(vv => vv.Value.Select(vvv => vvv.Value))))
                {
                    value.PropertyChanged += delegate { Interlocked.Increment(ref yieldCurveUpdatesSeen); };
                }

                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    var action = proc.PrepareUpdate();
                    Assert.Empty(action.Warnings);
                    Assert.Equal(0, Interlocked.Read(ref updatesSeen));
                    Assert.Equal(0, Interlocked.Read(ref yieldCurveUpdatesSeen));
                    action.Execute(updated);
                    if (Interlocked.Read(ref updatesSeen) + Interlocked.Read(ref yieldCurveUpdatesSeen) > 0)
                        break;
                }

                var afterCount = updated.Values.Count;
                Assert.Equal(beforeCount, afterCount);
                Console.Out.WriteLine(Interlocked.Read(ref updatesSeen) + " - " + Interlocked.Read(ref yieldCurveUpdatesSeen));
                Assert.NotEqual(0, Interlocked.Read(ref updatesSeen) + Interlocked.Read(ref yieldCurveUpdatesSeen));

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
        public void CanUpdateFromOtherView()
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            const string firstViewName = "Simple Swap Test View";
            using (var proc = snapshotManager.CreateFromViewDefinition(firstViewName))
            {
                Assert.Equal(0, proc.Snapshot.VolatilityCubes.Count);
                var curvesBefore = proc.Snapshot.YieldCurves.Count;
                Assert.NotEqual(0, curvesBefore);
                proc.Snapshot.BasisViewName = "Multi-Asset strategies view";
                Thread.Sleep(TimeSpan.FromSeconds(1));
                var action = proc.PrepareUpdate();
                action.Execute(proc.Snapshot);

                Assert.Equal(0, proc.Snapshot.VolatilityCubes.Count);
                Assert.NotEqual(0, proc.Snapshot.YieldCurves.Count);
                Assert.NotEqual(curvesBefore, proc.Snapshot.YieldCurves.Count);

                var changedPropertys = new HashSet<string>();
                proc.Snapshot.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                                                     { changedPropertys.Add(e.PropertyName); };
                proc.Snapshot.BasisViewName = firstViewName;
                action = proc.PrepareUpdate();
                action.Execute(proc.Snapshot);

                Assert.Equal(0, proc.Snapshot.VolatilityCubes.Count);
                Assert.Equal(curvesBefore, proc.Snapshot.YieldCurves.Count);
                Assert.DoesNotContain("VolatilityCubes", changedPropertys);
                Assert.Contains("YieldCurves", changedPropertys);
                Assert.Contains("BasisViewName", changedPropertys);
                Assert.Equal(2, changedPropertys.Count());
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
                    // TODO more strict
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

                                           Context.MarketDataSnapshotMaster.Update(new MarketDataSnapshotDocument(proc.Snapshot.UniqueId, proc.Snapshot));
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
                Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot));
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
                    Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot));
                }));
        }

        private bool CausesTick(Action<MarketDataSnapshotProcessor> action)
        {
            return CausesTick(u => ExecutionOptions.Snapshot(u.ToLatest()), action);
        }

        private bool CausesTick(Func<UniqueId, IViewExecutionOptions> optionsFactory, Action<MarketDataSnapshotProcessor> action)
        {
            const string viewName = RemoteViewClientBatchTests.ViewName;

            var snapshotManager = Context.MarketDataSnapshotManager;

            using (var proc = snapshotManager.CreateFromViewDefinition(viewName))
            {
                var manageableMarketDataSnapshot = proc.Snapshot;
                manageableMarketDataSnapshot.Name = TestUtils.GetUniqueName();
                var post = Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, manageableMarketDataSnapshot));
                manageableMarketDataSnapshot = post.Snapshot;
                var options = optionsFactory(manageableMarketDataSnapshot.UniqueId);

                var noTickOptions = GetNoTickOptions(options);

                using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
                {
                    var viewComputationResultModels = remoteViewClient.GetResults(GetViewDefinitionId(viewName), noTickOptions);
                    using (var enumerator = viewComputationResultModels.GetEnumerator())
                    {
                        Assert.True(enumerator.MoveNext());

                        var task = new Task<bool>(enumerator.MoveNext);
                        ((Task)task).ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);
                        task.Start();

                        Assert.False(task.Wait(TimeSpan.FromSeconds(15)));

                        action(proc);

                        return task.Wait(TimeSpan.FromSeconds(15));
                    }
                }
            }
        }

        private static IViewExecutionOptions GetNoTickOptions(IViewExecutionOptions options)
        {
            options = new ExecutionOptions(options.ExecutionSequence, 
                                           options.Flags & ~ViewExecutionFlags.TriggerCycleOnTimeElapsed, 
                                           options.MaxSuccessiveDeltaCycles, 
                                           options.DefaultExecutionOptions);
            return options;
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