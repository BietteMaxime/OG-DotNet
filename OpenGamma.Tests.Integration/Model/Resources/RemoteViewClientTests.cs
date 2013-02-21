// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewClientTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using OpenGamma.Core.Position;
using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Client;
using OpenGamma.Engine.View.Compilation;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Id;
using OpenGamma.Util;
using OpenGamma.Util.PublicAPI;
using OpenGamma.Xunit.Extensions;

using Xunit;
using Xunit.Extensions;

namespace OpenGamma.Model.Resources
{
    public class RemoteViewClientTests : ViewTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanCreate()
        {
            using (Context.ViewProcessor.CreateViewClient())
            {
            }
        }

        [Xunit.Extensions.Fact]
        public void AttachingToNullValuesThrowsException()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                Assert.Throws<ArgumentNullException>(() => remoteViewClient.AttachToViewProcess(null, ExecutionOptions.RealTime));
                Assert.Throws<ArgumentNullException>(() => remoteViewClient.AttachToViewProcess(null, null));
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAttachToViewDefinition()
        {
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                Assert.False(viewClient.IsAttached);
                viewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.RealTime);
                Assert.True(viewClient.IsAttached);
                viewClient.DetachFromViewProcess();
                Assert.False(viewClient.IsAttached);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAttachToExistingViewProcess()
        {
            using (var viewClient1 = Context.ViewProcessor.CreateViewClient())
            {
                Assert.False(viewClient1.IsAttached);
                viewClient1.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.RealTime, true);
                Assert.True(viewClient1.IsAttached);
                while (viewClient1.GetLatestResult() == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                var firstResult = viewClient1.GetLatestResult();

                using (var viewClient2 = Context.ViewProcessor.CreateViewClient())
                {
                    viewClient2.AttachToViewProcess(firstResult.ViewProcessId);
                    var secondResult = viewClient2.GetLatestResult();
                    Assert.NotNull(secondResult);
                    Assert.Equal(secondResult.ViewProcessId, firstResult.ViewProcessId);

                    viewClient1.TriggerCycle();
                    viewClient1.Dispose();

                    var thirdResult = viewClient2.GetLatestResult();
                    Assert.NotNull(thirdResult);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void Heartbeat()
        {
            using (var firstClient = Context.ViewProcessor.CreateViewClient())
            {
                var finished = new ManualResetEventSlim(false);
                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ProcessCompleted += delegate { finished.Set(); };
                firstClient.SetResultListener(eventViewResultListener); // Make sure the active connection is made
                firstClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.SingleCycle);
                finished.Wait(TimeSpan.FromMinutes(1));
                Thread.Sleep(TimeSpan.FromMinutes(1));
                Assert.NotNull(firstClient.GetUniqueId());
            }
        }

        [Xunit.Extensions.Fact]
        public void CanFailToDispose()
        {
            var firstClient = Context.ViewProcessor.CreateViewClient();
            var finished = new ManualResetEventSlim(false);
            var eventViewResultListener = new EventViewResultListener();
            eventViewResultListener.ProcessCompleted += delegate { finished.Set(); };
            firstClient.SetResultListener(eventViewResultListener); // Make sure the active connection is made
            firstClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.SingleCycle);
            finished.Wait(TimeSpan.FromMinutes(1));
            var reference = new WeakReference(firstClient);

            GC.Collect();
            if (reference.IsAlive)
            {
                throw new Exception("Should have GCed " + reference.Target);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetUid()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                Assert.NotNull(remoteViewClient.GetUniqueId());
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetIsLiveComputationRunning(ViewDefinition definition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                Assert.Equal(ViewClientState.Started, remoteViewClient.GetState());
                remoteViewClient.Pause();
                Assert.Equal(ViewClientState.Paused, remoteViewClient.GetState());
                remoteViewClient.Resume();
                Assert.Equal(ViewClientState.Started, remoteViewClient.GetState());
            }
        }

        [Xunit.Extensions.Fact]
        public void CanStartAndGetAResultOrCompilationFailure(ViewDefinition definition)
        {
            try
            {
                Assert.NotNull(GetOneResultCache.Get(definition.UniqueId));
            }
            catch (Exception ex)
            {
                // Compilation failure is allowed, we will check that some views run elsewhere
                Assert.Contains("ViewDefinitionCompilationFailedArgs", ex.Message);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetCompilationResults(ViewDefinition definition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var compilationResult = new BlockingCollection<object>();

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ViewDefinitionCompiled +=
                    (sender, e) => compilationResult.Add(e.CompiledViewDefinition);
                eventViewResultListener.ViewDefinitionCompilationFailed +=
                    (sender, e) => compilationResult.Add(e.Exception);

                remoteViewClient.SetResultListener(eventViewResultListener);
                remoteViewClient.AttachToViewProcess(definition.UniqueId, ExecutionOptions.GetCompileOnly());

                var result = compilationResult.Take();
                Assert.IsNotType(typeof(Exception), result);
                Debug.WriteLine(definition.UniqueId);
                Assert.IsAssignableFrom(typeof(ICompiledViewDefinition), result);

                var viewDefin = (ICompiledViewDefinition)result;
                ValueAssertions.AssertSensibleValue(viewDefin);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetManyResults(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var options = ExecutionOptions.RealTime;
                var resultsEnum = remoteViewClient.GetResults(viewDefinition.UniqueId, options);

                var results = resultsEnum.Take(5).ToList();
                Assert.True(results.All(r => r != null));
            }
        }

        public IEnumerable<string> DataSources
        {
            get { return Context.ViewProcessor.LiveMarketDataSourceRegistry.GetNames(); }
        }
        
        [Xunit.Extensions.Fact]
        [TypedPropertyData("DataSources")]
        public void CanSpecifyLiveData(string dataSource)
        {
            if (!dataSource.ToLower().Contains("bloomberg"))
            {
                // Assume tests are Bloomberg-based
                return;
            }

            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var spec = new LiveMarketDataSpecification(dataSource);
                var marketDataSpecifications = new List<MarketDataSpecification> {spec};
                var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, null, new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications));
                var resultsEnum = remoteViewClient.GetResults(Fixture.EquityViewDefinition.UniqueId, options);

                var results = resultsEnum.Take(1).ToList();
                Assert.True(results.All(r => r != null));
                Console.Out.WriteLine("{0}: {1}", dataSource, string.Join(",", results.SelectMany(r => r.AllLiveData.Select(d => d.Specification.TargetSpecification.UniqueId.ToString())).OrderBy(u => u)));
            }
        }

        [Xunit.Extensions.Fact]
        public void NumberOfResultsIsConsistent()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var marketDataSpecifications = new List<MarketDataSpecification> {new LiveMarketDataSpecification()};
                var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, defaultExecutionOptions: new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications));
                var resultsEnum = remoteViewClient.GetResults(Fixture.EquityViewDefinition.UniqueId, options);

                var results = resultsEnum.Take(3).ToList();
                AssertNumberOfResultsIsConsistentOnRecompile(results);
            }
        }

        [Xunit.Extensions.Fact]
        public void NumberOfResultsIsConsistentOnRecompile()
        {
            var results = new List<IViewComputationResultModel>();
            var viewProcess = new HashSet<UniqueId>();
            for (int i = 0; i < 10; i++)
            {
                using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
                {
                    var options = ExecutionOptions.SingleCycle;
                    var resultsEnum = remoteViewClient.GetResults(Fixture.EquityViewDefinition.UniqueId, options, true);

                    var result = resultsEnum.Take(1).ToList().Single();
                    if (!viewProcess.Add(result.ViewProcessId))
                    {
                        throw new Exception("Shared process");
                    }

                    results.Add(result);
                }
            }

            AssertNumberOfResultsIsConsistentOnRecompile(results);
        }

        [Xunit.Extensions.Theory]
        [EnumValuesData]
        public void CanSetViewResultMode(ViewResultMode mode)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                remoteViewClient.SetResultMode(mode);

                var options = ExecutionOptions.RealTime;
                var resultsEnum = remoteViewClient.GetCycles(Fixture.EquityViewDefinition.UniqueId, options);

                var results = resultsEnum.Take(3).ToList();
                Matches(mode, results);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetDefinition()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var options = ExecutionOptions.RealTime;
                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, options);
                for (int i = 0; i < 30; i++)
                {
                    var viewDefinition = remoteViewClient.GetViewDefinition();
                    if (viewDefinition != null)
                    {
                        ValueAssertions.AssertSensibleValue(viewDefinition);
                        return;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                Assert.True(false, "Failed to get view definition");
            }
        }

        [Xunit.Extensions.Theory]
        [InlineData("10:00:00Z", "11:00:00Z")]
        [InlineData(null, null)]
        public void CanGetVersionCorrection(string versionAsOf, string correctedTo)
        {
            VersionCorrection vc = versionAsOf == null ? VersionCorrection.Latest : new VersionCorrection(DateTimeOffset.Parse(versionAsOf), DateTimeOffset.Parse(correctedTo));
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.CompileOnly, versionCorrection: vc);
                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, options);

                for (int i = 0; i < 30; i++)
                {
                    var viewDefinition = remoteViewClient.GetViewDefinition();
                    if (viewDefinition != null)
                    {
                        ValueAssertions.AssertSensibleValue(viewDefinition);
                        var roundTripped = remoteViewClient.GetProcessVersionCorrection();
                        Assert.Equal(vc, roundTripped);
                        return;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                Assert.True(false, "Failed to get view definition");
            }
        }

        [Xunit.Extensions.Fact]
        public void CanSetUpdatePeriod()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                remoteViewClient.SetUpdatePeriod(200);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetResultByPolling()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.SingleCycle);

                while (true)
                {
                    var latestResult = remoteViewClient.GetLatestResult();
                    if (remoteViewClient.IsResultAvailable)
                        break;
                    Assert.Null(latestResult);

                    Thread.Sleep(100);
                }

                Assert.NotNull(remoteViewClient.GetLatestResult());
            }
        }

        [Xunit.Extensions.Fact]
        public void ViewResultsMatchDefinition()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                ICompiledViewDefinition compiledViewDefinition = null;
                IViewComputationResultModel viewComputationResultModel = null;
                var resultsReady = new ManualResetEvent(false);

                var listener = new EventViewResultListener();
                listener.ViewDefinitionCompiled += delegate(object sender, ViewDefinitionCompiledArgs e)
                                                       {
                                                           compiledViewDefinition = e.CompiledViewDefinition;
                                                       };
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                                               {
                                                   viewComputationResultModel = e.FullResult;
                                                   resultsReady.Set();
                                               };
                remoteViewClient.SetResultListener(listener);

                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.RealTime);

                if (!resultsReady.WaitOne(TimeSpan.FromMinutes(2)))
                    throw new TimeoutException("Failed to get results for client " + remoteViewClient.GetUniqueId());

                Assert.NotNull(compiledViewDefinition);

                foreach (var viewResultEntry in viewComputationResultModel.AllResults)
                {
                    Assert.NotNull(viewResultEntry.ComputedValue.Value);
                    AssertDefinitionContains(Fixture.EquityViewDefinition, viewResultEntry);
                }

                var countActualValues = viewComputationResultModel.AllResults.Count();

                var countMaxExpectedValues = CountMaxExpectedValues(compiledViewDefinition);

                Console.Out.WriteLine("{1} {2}", countActualValues, countMaxExpectedValues);
                Assert.InRange(countActualValues, 1, countMaxExpectedValues);
            }
        }

        /// <summary>
        /// NOTE: PLAT-1248 makes this less useful than it might be
        /// </summary>
        [Xunit.Extensions.Fact]
        public void CanReAttach()
        {
            var timeout = TimeSpan.FromMinutes(0.5);

            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var compilationResult = new BlockingCollection<ICompiledViewDefinition>();
                var compilationError = new BlockingCollection<JavaException>();

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ViewDefinitionCompiled +=
                    (sender, e) => compilationResult.Add(e.CompiledViewDefinition);
                eventViewResultListener.ViewDefinitionCompilationFailed +=
                    (sender, e) => compilationError.Add(e.Exception);

                remoteViewClient.SetResultListener(eventViewResultListener);
                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.RealTime);

                ICompiledViewDefinition result;

                Assert.True(compilationResult.TryTake(out result, timeout));
                Assert.Equal(Fixture.EquityViewDefinition.UniqueId, result.ViewDefinition.UniqueId);
                remoteViewClient.DetachFromViewProcess();
                Assert.False(remoteViewClient.IsAttached);
                remoteViewClient.AttachToViewProcess(Fixture.EquityViewDefinition.UniqueId, ExecutionOptions.RealTime);
                Assert.True(compilationResult.TryTake(out result, timeout));
                Assert.Equal(Fixture.EquityViewDefinition.UniqueId, result.ViewDefinition.UniqueId);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetCycleSupport()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                remoteViewClient.SetViewCycleAccessSupported(false);
                Assert.Equal(false, remoteViewClient.GetViewCycleAccessSupported());
                remoteViewClient.SetViewCycleAccessSupported(true);
                Assert.Equal(true, remoteViewClient.GetViewCycleAccessSupported());
            }
        }

        [Xunit.Extensions.Fact]
        public void ViewResultsHaveSaneValues()
        {
            var viewComputationResultModel = GetOneResultCache.Get(Fixture.EquityViewDefinition.UniqueId);

            foreach (var viewResultEntry in viewComputationResultModel.AllResults)
            {
                ValueAssertions.AssertSensibleValue(viewResultEntry.ComputedValue.Value);
            }
        }

        private static void Matches(ViewResultMode mode, IEnumerable<CycleCompletedArgs> results)
        {
            switch (mode)
            {
                case ViewResultMode.FullOnly:
                    Assert.True(results.All(r => r.DeltaResult == null));
                    Assert.True(results.All(r => r.FullResult != null));
                    break;
                case ViewResultMode.DeltaOnly:
                    Assert.True(results.Skip(1).All(r => r.DeltaResult != null));
                    Assert.True(results.All(r => r.FullResult == null));
                    break;
                case ViewResultMode.FullThenDelta:
                    Matches(ViewResultMode.FullOnly, results.Take(1));
                    Matches(ViewResultMode.DeltaOnly, results.Skip(1));
                    break;
                case ViewResultMode.Both:
                    Assert.True(results.Skip(1).All(r => r.DeltaResult != null));
                    Assert.True(results.All(r => r.FullResult != null));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }

            CheckResults(results.SelectMany(c => new IViewResultModel[] { c.FullResult, c.DeltaResult }).Where(r => r != null));
            CheckDeltas(results, mode);
        }

        private static void CheckResults(IEnumerable<IViewResultModel> resultModels)
        {
            foreach (var results in resultModels)
            {
                Assert.NotNull(results.ViewCycleId);
                Assert.NotNull(results.ViewProcessId);
            }
        }

        private static void CheckDeltas(IEnumerable<CycleCompletedArgs> results, ViewResultMode mode)
        {
            var deltas = results.Select(r => r.DeltaResult).ToList();

            switch (mode)
            {
                case ViewResultMode.Both:
                case ViewResultMode.DeltaOnly:

                    // Assert.Equal(default(DateTimeOffset), deltas.First().PreviousResultTimestamp);
                    Assert.True(deltas.Skip(1).All(d => d.PreviousResultTimestamp != default(DateTimeOffset)));
                    break;
                case ViewResultMode.FullThenDelta:
                    Assert.True(deltas.Skip(1).All(d => d.PreviousResultTimestamp != default(DateTimeOffset)));
                    break;
                case ViewResultMode.FullOnly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }

            for (int i = 1; i < deltas.Count(); i++)
            {
                var prevDelta = deltas[i - 1];
                if (prevDelta != null)
                {
                    var prevTime = prevDelta.ResultTimestamp;
                    Assert.Equal(prevTime, deltas[i].PreviousResultTimestamp);
                }
            }
        }

        private static readonly Memoizer<UniqueId, IViewComputationResultModel> GetOneResultCache = new Memoizer<UniqueId, IViewComputationResultModel>(GetOneResult);

        private static IViewComputationResultModel GetOneResult(UniqueId viewDefinitionId)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
            {
                var options = ExecutionOptions.SingleCycle;
                return remoteViewClient.GetResults(viewDefinitionId, options).First();
            }
        }

        private static void AssertDefinitionContains(ViewDefinition viewDefinition, ViewResultEntry viewResultEntry)
        {
            var configuration = viewDefinition.CalculationConfigurationsByName[viewResultEntry.CalculationConfiguration];

            var valueSpecification = viewResultEntry.ComputedValue.Specification;

            foreach (var req in configuration.SpecificRequirements)
            {
                bool matches = (req.TargetReference.Specification.UniqueId ==
                                valueSpecification.TargetSpecification.UniqueId
                                && req.TargetReference.Type == valueSpecification.TargetSpecification.Type
                                && req.ValueName == valueSpecification.ValueName)
                               && req.Constraints.IsSatisfiedBy(valueSpecification.Properties);

                if (matches)
                    return;
            }

            var reqsByType = configuration.PortfolioRequirementsBySecurityType;
            if (!reqsByType.Any(r => r.Value.Any(t => t.Item1 == valueSpecification.ValueName)))
            {
                Assert.True(false, 
                            string.Format("Unmatched requirement {0},{1},{2} on {3}", valueSpecification.ValueName, 
                                          valueSpecification.TargetSpecification.Type, 
                                          valueSpecification.TargetSpecification.UniqueId, viewDefinition.Name));
            }
        }

        private static int CountMaxExpectedValues(ICompiledViewDefinition compiledDefinition)
        {
            var specifics = compiledDefinition.ViewDefinition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.SpecificRequirements.Count());
            int rows = CountRows(compiledDefinition.Portfolio);
            var values = rows * compiledDefinition.ViewDefinition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.PortfolioRequirementsBySecurityType.Sum(r => r.Value.Count));
            return specifics + values;
        }

        private static int CountRows(IPortfolio portfolio)
        {
            if (portfolio == null)
                return 0;
            return CountRows(portfolio.Root);
        }

        private static int CountRows(IPortfolioNode portfolio)
        {
            return (1 + portfolio.Positions.Count()) + portfolio.ChildNodes.Sum(n => CountRows(n));
        }

        private static void AssertNumberOfResultsIsConsistentOnRecompile(IEnumerable<IViewComputationResultModel> results)
        {
            HashSet<Tuple<string, ValueSpecification>> values = null;
            foreach (var result in results)
            {
                Assert.NotNull(result);
                var newValues = new HashSet<Tuple<string, ValueSpecification>>(result.AllResults.Select(r => Tuple.Create(r.CalculationConfiguration, r.ComputedValue.Specification)));
                if (values != null)
                {
                    if (!values.SetEquals(newValues))
                    {
                        var missing = values.Except(newValues).ToList();
                        var added = newValues.Except(values).ToList();
                        throw new Exception(string.Format("Previously missing {0} results, now {1}. Missing {2}. Added {3}", values.Count, newValues.Count, string.Join(",", missing), string.Join(",", added)));
                    }
                }
                else
                {
                    values = newValues;
                }
            }
        }
    }
}
