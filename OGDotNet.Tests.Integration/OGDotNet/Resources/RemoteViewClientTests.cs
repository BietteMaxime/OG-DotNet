//-----------------------------------------------------------------------
// <copyright file="RemoteViewClientTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.util.PublicAPI;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
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

        [Xunit.Extensions.Theory]
        [TypedPropertyData("ViewDefinitions")]
        public void CanAttach(ViewDefinition vd)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                Assert.False(remoteViewClient.IsAttached);
                remoteViewClient.AttachToViewProcess(vd.Name, new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), true, true, null, false));
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
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetManyResults(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), true, true, null, false);
                var resultsEnum = remoteViewClient.GetResults(viewDefinition.Name, options);

                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    enumerator.MoveNext();
                    Assert.NotNull(enumerator.Current);

                    for (int i = 0; i < 5; i++)
                    {
                        var requested = DateTimeOffset.Now;
                        enumerator.MoveNext();
                        Assert.NotNull(enumerator.Current);

                        var valuation = enumerator.Current.ValuationTime.ToDateTimeOffset();
                        var result = enumerator.Current.ResultTimestamp.ToDateTimeOffset();
                        var now = DateTimeOffset.Now;

                        //Make sure we're not being shown up too much by the server
                        var timeToReceive = now - requested;
                        var timeToCalculate = result - valuation;
                        if (!Debugger.IsAttached)
                        {
                            Assert.InRange(timeToReceive, TimeSpan.Zero, TimeSpan.FromMilliseconds(timeToCalculate.TotalMilliseconds * 3.0));
                        }
                    }
                }
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanPauseAndRestart(ViewDefinition viewDefinition)
        {
            Console.WriteLine(string.Format("Checking view {0}", viewDefinition.Name));
            const int forbiddenAfterPause = 3;

            var timeout = TimeSpan.FromMilliseconds(20000);

            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), true, true, null, false);
                var resultsEnum = remoteViewClient.GetResults(viewDefinition.Name, options);

                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    bool endOfStream = false;
                    Action act = delegate
                                     {
                                         if (enumerator.MoveNext())
                                         {
                                             Assert.NotNull(enumerator.Current);
                                         }
                                         else
                                         {
                                             endOfStream = true;
                                         }
                                     };

                    IAsyncResult pendingResult = null;

                    Assert.True(InvokeWithTimeout(act, timeout, ref pendingResult));
                    Assert.False(endOfStream);
                    remoteViewClient.Pause();
                    {
                        int got = 0;
                        for (int i = 0; i < forbiddenAfterPause; i++)
                        {
                            if (InvokeWithTimeout(act, timeout, ref pendingResult))
                            {
                                got++;
                                Assert.False(endOfStream);
                            }
                            else
                            {
                                break;
                            }
                        }
                        Assert.True(got < forbiddenAfterPause,
                                    string.Format("I got {0} results for view {1} within {2} after pausing", got,
                                                  viewDefinition.Name, timeout));
                    }
                    remoteViewClient.Resume();
                    {
                        int got = 0;
                        for (int i = 0; i < forbiddenAfterPause; i++)
                        {
                            if (InvokeWithTimeout(act, timeout, ref pendingResult))
                            {
                                Assert.False(endOfStream);
                                got++;
                            }
                        }
                        Assert.True(got == forbiddenAfterPause,
                                    string.Format("I got {0} results for view {1} within {2} after pausing", got,
                                                  viewDefinition.Name, timeout));
                    }

                    Assert.True(InvokeWithTimeout(act, timeout, ref pendingResult));
                    Assert.True(endOfStream);
                }
            }
            Console.WriteLine(string.Format("Checked view {0}", viewDefinition.Name));
        }

        private static bool InvokeWithTimeout(Action act, TimeSpan timeout, ref IAsyncResult result)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            result = result ?? act.BeginInvoke(null, null);
            var waitOne = result.AsyncWaitHandle.WaitOne(timeout);
            stopwatch.Stop();
            if (waitOne)
            {
                Console.Out.WriteLine("Invoked after {0}", stopwatch.Elapsed);
                act.EndInvoke(result);
                result = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        static readonly Memoizer<string, InMemoryViewComputationResultModel> GetOneResultCache = new Memoizer<string, InMemoryViewComputationResultModel>(GetOneResult);
        private static InMemoryViewComputationResultModel GetOneResult(string viewDefinitionName)
        {

            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var options = new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), true, true, null, false);
                remoteViewClient.AttachToViewProcess(viewDefinitionName, options);
                Assert.False(remoteViewClient.IsCompleted);
                //TODO
                while ( ! remoteViewClient.IsResultAvailable)
                {
                    Thread.Sleep(1000);    
                }
                var resultsEnum = remoteViewClient.GetLatestResult();
                return resultsEnum;
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void ViewResultsMatchDefinition(ViewDefinition viewDefinition)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                ICompiledViewDefinition compiledViewDefinition = null;
                InMemoryViewComputationResultModel viewComputationResultModel = null;
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

                remoteViewClient.AttachToViewProcess(viewDefinition.Name, ExecutionOptions.Live);

                if (!resultsReady.WaitOne(TimeSpan.FromSeconds(15)))
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
