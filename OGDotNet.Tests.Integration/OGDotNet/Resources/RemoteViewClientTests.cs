//-----------------------------------------------------------------------
// <copyright file="RemoteViewClientTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Engine.MarketData.Spec;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Client;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.PublicAPI;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using OGDotNet.Utils;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
            }
        }

        [Xunit.Extensions.Fact]
        public void CantAttachToNullValues()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                Assert.Throws<ArgumentNullException>(() => remoteViewClient.AttachToViewProcess(null, ExecutionOptions.RealTime));
                Assert.Throws<ArgumentNullException>(() => remoteViewClient.AttachToViewProcess("SomeView", null));
            }
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void CanAttach(ViewDefinition vd)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                Assert.False(remoteViewClient.IsAttached);
                remoteViewClient.AttachToViewProcess(vd.Name, ExecutionOptions.RealTime);
                Assert.True(remoteViewClient.IsAttached);
                remoteViewClient.DetachFromViewProcess();
                Assert.False(remoteViewClient.IsAttached);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAttachToViewProcess()
        {
            const string vdName = "Equity Option Test View 1";
            using (var firstClient = Context.ViewProcessor.CreateClient())
            {
                Assert.False(firstClient.IsAttached);
                firstClient.AttachToViewProcess(vdName, ExecutionOptions.RealTime, true);
                Assert.True(firstClient.IsAttached);
                while (firstClient.GetLatestResult() == null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                var firstResult = firstClient.GetLatestResult();

                using (var secondClient = Context.ViewProcessor.CreateClient())
                {
                    secondClient.AttachToViewProcess(firstResult.ViewProcessId);
                    var secondResult = secondClient.GetLatestResult();
                    Assert.NotNull(secondResult);
                    Assert.Equal(secondResult.ViewProcessId, firstResult.ViewProcessId);

                    firstClient.TriggerCycle();
                    firstClient.Dispose();

                    var thirdResult = secondClient.GetLatestResult();
                    Assert.NotNull(thirdResult);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetUid()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                Assert.NotNull(remoteViewClient.GetUniqueId());
            }
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void CanGetIsLiveComputationRunning(ViewDefinition definition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                Assert.Equal(ViewClientState.Started, remoteViewClient.GetState());
                remoteViewClient.Pause();
                Assert.Equal(ViewClientState.Paused, remoteViewClient.GetState());
                remoteViewClient.Resume();
                Assert.Equal(ViewClientState.Started, remoteViewClient.GetState());
            }
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void CanStartAndGetAResult(ViewDefinition definition)
        {
            Assert.NotNull(GetOneResultCache.Get(definition.Name));
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void CanGetCompilationResults(ViewDefinition definition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var compilationResult = new BlockingCollection<object>();

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ViewDefinitionCompiled +=
                    (sender, e) => compilationResult.Add(e.CompiledViewDefinition);
                eventViewResultListener.ViewDefinitionCompilationFailed +=
                    (sender, e) => compilationResult.Add(e.Exception);

                remoteViewClient.SetResultListener(eventViewResultListener);
                remoteViewClient.AttachToViewProcess(definition.Name, ExecutionOptions.GetCompileOnly());

                var result = compilationResult.Take();
                Assert.IsNotType(typeof(Exception), result);

                var viewDefin = (ICompiledViewDefinition)result;
                ValueAssertions.AssertSensibleValue(viewDefin);
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetManyResults(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = ExecutionOptions.RealTime;
                var resultsEnum = remoteViewClient.GetResults(viewDefinition.Name, options);

                var results = resultsEnum.Take(5).ToList();
                Assert.True(results.All(r => r != null));
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void NumberOfResultsIsConsistent(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, defaultExecutionOptions:new ViewCycleExecutionOptions(default(DateTimeOffset), new LiveMarketDataSpecification()));
                var resultsEnum = remoteViewClient.GetResults(viewDefinition.Name, options);

                var results = resultsEnum.Take(200).ToList();
                Assert.True(results.All(r => r != null));
                var counts = results.Select(r => r.AllResults.Count());
                if (counts.Distinct().Count() != 1)
                {
                    throw new Exception(string.Format("Inconsistent number of results for {0} {1}", viewDefinition.Name, string.Join(",", counts.Select(c => c.ToString()))));
                }
                Assert.Equal(1, counts.Distinct().Count());
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void NumberOfResultsIsConsistentOnRecompile(ViewDefinition viewDefinition)
        {
            int? counts = null;
            var viewProcess = new HashSet<UniqueId>();
            for (int i = 0; i < 30; i++)
            {
                using (var remoteViewClient = Context.ViewProcessor.CreateClient())
                {
                    var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, defaultExecutionOptions: new ViewCycleExecutionOptions(default(DateTimeOffset), new LiveMarketDataSpecification()));
                    var resultsEnum = remoteViewClient.GetResults(viewDefinition.Name, options, true);

                    var results = resultsEnum.Take(1).ToList();
                    var result = results.Single();
                    if (!viewProcess.Add(result.ViewProcessId))
                    {
                        throw new Exception("Shared process");
                    }
                    Assert.NotNull(result);
                    int newCount = result.AllResults.Count();
                    if (counts.HasValue)
                    {
                        Assert.Equal(counts.Value, newCount);
                    }
                    else
                    {
                        counts = newCount;
                    }
                }
            }
        }

        [Theory]
        [EnumValuesData]
        public void CanSetViewResultMode(ViewResultMode mode)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                remoteViewClient.SetViewResultMode(mode);

                var options = ExecutionOptions.RealTime;
                var resultsEnum = remoteViewClient.GetCycles("Equity Option Test View 1", options);

                var results = resultsEnum.Take(3).ToList();
                Matches(mode, results);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetDefinition()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = ExecutionOptions.RealTime;
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", options);
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

        [Xunit.Extensions.Fact]
        public void CanSetUpdatePeriod()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                remoteViewClient.SetUpdatePeriod(200);
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
                    //Assert.Equal(default(DateTimeOffset), deltas.First().PreviousResultTimestamp);
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

        [Xunit.Extensions.Fact]
        public void CanGetResultByPolling()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", ExecutionOptions.SingleCycle);

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

        static readonly Memoizer<string, IViewComputationResultModel> GetOneResultCache = new Memoizer<string, IViewComputationResultModel>(GetOneResult);
        private static IViewComputationResultModel GetOneResult(string viewDefinitionName)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = ExecutionOptions.SingleCycle;
                return remoteViewClient.GetResults(viewDefinitionName, options).First();
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void ViewResultsMatchDefinition(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                ICompiledViewDefinition compiledViewDefinition = null;
                IViewComputationResultModel viewComputationResultModel = null;
                ManualResetEvent resultsReady = new ManualResetEvent(false);

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

                remoteViewClient.AttachToViewProcess(viewDefinition.Name, ExecutionOptions.RealTime);

                if (!resultsReady.WaitOne(TimeSpan.FromMinutes(2)))
                    throw new TimeoutException("Failed to get results for " + viewDefinition.Name + " client " + remoteViewClient.GetUniqueId());

                Assert.NotNull(compiledViewDefinition);

                foreach (var viewResultEntry in viewComputationResultModel.AllResults)
                {
                    Assert.NotNull(viewResultEntry.ComputedValue.Value);
                    AssertDefinitionContains(viewDefinition, viewResultEntry);
                }

                var countActualValues = viewComputationResultModel.AllResults.Count();

                var countMaxExpectedValues = CountMaxExpectedValues(compiledViewDefinition);

                Console.Out.WriteLine("{0} {1} {2}", viewDefinition.Name, countActualValues, countMaxExpectedValues);
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

            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var compilationResult = new BlockingCollection<ICompiledViewDefinition>();
                var compilationError = new BlockingCollection<JavaException>();

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ViewDefinitionCompiled +=
                    (sender, e) => compilationResult.Add(e.CompiledViewDefinition);
                eventViewResultListener.ViewDefinitionCompilationFailed +=
                    (sender, e) => compilationError.Add(e.Exception);

                remoteViewClient.SetResultListener(eventViewResultListener);
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", ExecutionOptions.RealTime);

                ICompiledViewDefinition result;

                Assert.True(compilationResult.TryTake(out result, timeout));
                Assert.Equal("Equity Option Test View 1", result.ViewDefinition.Name);
                remoteViewClient.DetachFromViewProcess();
                remoteViewClient.AttachToViewProcess("Equity Option Test View 2", ExecutionOptions.RealTime);
                Assert.True(compilationResult.TryTake(out result, timeout));
                Assert.Equal("Equity Option Test View 2", result.ViewDefinition.Name);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetCycleSupport()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                remoteViewClient.SetViewCycleAccessSupported(false);
                Assert.Equal(false, remoteViewClient.GetViewCycleAccessSupported());
                remoteViewClient.SetViewCycleAccessSupported(true);
                Assert.Equal(true, remoteViewClient.GetViewCycleAccessSupported());
            }
        }

        [Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void ViewResultsHaveSaneValues(ViewDefinition definition)
        {
            var viewComputationResultModel = GetOneResultCache.Get(definition.Name);

            foreach (var viewResultEntry in viewComputationResultModel.AllResults)
            {
                ValueAssertions.AssertSensibleValue(viewResultEntry.ComputedValue.Value);
            }
        }

        private static void AssertDefinitionContains(ViewDefinition viewDefinition, ViewResultEntry viewResultEntry)
        {
            var configuration = viewDefinition.CalculationConfigurationsByName[viewResultEntry.CalculationConfiguration];

            var valueSpecification = viewResultEntry.ComputedValue.Specification;

            foreach (var req in configuration.SpecificRequirements)
            {
                bool matches = (req.TargetSpecification.Uid ==
                                valueSpecification.TargetSpecification.Uid
                                && req.TargetSpecification.Type == valueSpecification.TargetSpecification.Type
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
                                          valueSpecification.TargetSpecification.Uid, viewDefinition.Name));
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

        private static int CountRows(PortfolioNode portfolio)
        {
            return (1 + portfolio.Positions.Count()) + portfolio.SubNodes.Sum(n => CountRows(n));
        }
    }
}
