//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManagerTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanCreateAndRunFromView()
        {
            //LAPANA-50 : this test should cover all viewdefns
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromViewDefinition(RemoteViewClientBatchTests.ViewName)) 
            {
                proc.Snapshot.Name = TestUtils.GetUniqueName();
                Context.MarketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, proc.Snapshot));

                var options = ExecutionOptions.SingleCycle;
                var withoutSnapshot = RemoteViewClientBatchTests.RunToCompletion(options);

                options = ExecutionOptions.Snapshot(proc.Snapshot.UniqueId);
                var withSnapshot = RemoteViewClientBatchTests.RunToCompletion(options);

                Assert.Equal(CountResults(withoutSnapshot), CountResults(withSnapshot));
            }
        }

        private static int CountResults(Tuple<IEnumerable<ViewDefinitionCompiledArgs>, IEnumerable<CycleCompletedArgs>> withSnapshot)
        {
            return withSnapshot.Item2.Select(r => r.FullResult.AllResults.Count()).Sum();
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
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
                action.Execute();
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
                                           action.Execute();

                                           Context.MarketDataSnapshotMaster.Update((new MarketDataSnapshotDocument(proc.Snapshot.UniqueId, proc.Snapshot)));
                                       }));
        }

        [Xunit.Extensions.Fact]
        public void AddingSnapshotDoesntTickView()
        {
            Assert.False(CausesTick(delegate(MarketDataSnapshotProcessor proc)
            {
                var action = proc.PrepareUpdate();
                action.Execute();

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
                    action.Execute();

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