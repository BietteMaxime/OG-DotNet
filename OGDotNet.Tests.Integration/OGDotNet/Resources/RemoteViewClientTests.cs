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
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.client;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.util.PublicAPI;
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

            CheckResults(results.SelectMany(c=>new IViewResultModel[]{c.FullResult, c.DeltaResult}).Where(r=>r!=null));
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
                if (deltas[i-1] != null)
                {
                    var prevTime = deltas[i - 1].ResultTimestamp;
                    Assert.Equal(prevTime, deltas[i].PreviousResultTimestamp);    
                }
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetResultByPolling(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                remoteViewClient.AttachToViewProcess(viewDefinition.Name, ExecutionOptions.SingleCycle);

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

        static readonly Memoizer<string, ViewComputationResultModel> GetOneResultCache = new Memoizer<string, ViewComputationResultModel>(GetOneResult);
        private static ViewComputationResultModel GetOneResult(string viewDefinitionName)
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
                ViewComputationResultModel viewComputationResultModel = null;
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
            var timeout = TimeSpan.FromSeconds(10);

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

        [Xunit.Extensions.Fact]
        public void CanGetCycle()
        {
            using (var executedMre = new ManualResetEventSlim(false))
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { executedMre.Set(); };
                
                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", ExecutionOptions.SingleCycle);
                Assert.Null(remoteViewClient.CreateLatestCycleReference());

                executedMre.Wait(TimeSpan.FromMinutes(1));

                using (var engineResourceReference = remoteViewClient.CreateLatestCycleReference())
                {
                    Assert.NotNull(engineResourceReference.Value.UniqueId);
                    var resultModel = engineResourceReference.Value.GetResultModel();
                    Assert.NotNull(resultModel);

                    Assert.Throws<ArgumentException>(() => engineResourceReference.Value.QueryComputationCaches(new ComputationCacheQuery("Default")));

                    var computedValue = resultModel.AllResults.First().ComputedValue;
                    var valueSpec = computedValue.Specification;

                    var nonEmptyResponse = engineResourceReference.Value.QueryComputationCaches(new ComputationCacheQuery("Default", valueSpec));

                    Assert.NotNull(nonEmptyResponse);

                    var results = nonEmptyResponse.Results;
                    Assert.NotEmpty(results);
                    Assert.Equal(1, results.Count());
                    Assert.Equal(computedValue.Specification, results.Single().First);
                    Assert.Equal(computedValue.Value, results.Single().Second);
                }
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
            foreach (var valuePropertiese in reqsByType)
            {
                var hashSet = valuePropertiese.Value;
                if (!hashSet.Any(t => t.Item1 == valueSpecification.ValueName))
                {
                    Assert.True(false, string.Format("Unmatched requirement {0},{1},{2} on {3}", valueSpecification.ValueName,
                                                     valueSpecification.TargetSpecification.Type,
                                                     valueSpecification.TargetSpecification.Uid, viewDefinition.Name));
                }
            }
        }

        private static int CountMaxExpectedValues(ICompiledViewDefinition compiledDefinition)
        {
            var specifics = compiledDefinition.ViewDefinition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.SpecificRequirements.Count());
            int rows = CountRows(compiledDefinition.Portfolio);
            var values = rows * compiledDefinition.ViewDefinition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.PortfolioRequirementsBySecurityType.Single().Value.Count);
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
