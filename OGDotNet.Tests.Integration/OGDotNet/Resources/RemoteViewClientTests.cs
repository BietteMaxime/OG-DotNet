using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientTests: ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("Views")]
        public void CanGet(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                Assert.NotNull(remoteViewClient);
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanStartAndGetAResult(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                foreach (var viewComputationResultModel in resultsEnum)
                {
                    Assert.NotNull(viewComputationResultModel);
                    break;
                }
            }
        }

        [Theory(Skip = "The timing stuff needs to be fixed to work with clock drift")]
        [TypedPropertyData("Views")]
        public void CanGetManyResults(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    enumerator.MoveNext();
                    Assert.NotNull(enumerator.Current);

                    for (int i = 0; i < 3; i++)
                    {
                        enumerator.MoveNext();
                        Assert.NotNull(enumerator.Current);

                        
                        var valuation = enumerator.Current.ValuationTime.ToDateTimeOffset();
                        var result = enumerator.Current.ResultTimestamp.ToDateTimeOffset();
                        var now = DateTimeOffset.Now;

                        var timeToTransmit = now-result;
                        var timeToCalculate = result-valuation;
                        Assert.InRange(timeToTransmit, TimeSpan.Zero, timeToCalculate);
                    }
                }
            }
        }


        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void CanPauseAndRestart(RemoteView view)
        {
            Console.WriteLine(string.Format("Checking view {0}", view.Name));
            const int forbiddenAfterPause = 3;

            var timeout = TimeSpan.FromMilliseconds(60000);

            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                using (var cts = new CancellationTokenSource())
                {
                    var resultsEnum = remoteViewClient.GetResults(cts.Token);

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
                            for (int i = 0; i < forbiddenAfterPause;i++)
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
                            Assert.True(got < forbiddenAfterPause, string.Format("I got {0} results for view {1} within {2} after pausing", got, view.Name, timeout));
                        }
                        remoteViewClient.Start();
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
                            Assert.True(got == forbiddenAfterPause, string.Format("I got {0} results for view {1} within {2} after pausing", got, view.Name, timeout));
                        }

                        cts.Cancel();

                        Assert.True(InvokeWithTimeout(act, timeout, ref pendingResult));
                        Assert.True(endOfStream);
                    }
                }
            }
            Console.WriteLine(string.Format("Checked view {0}", view.Name));
        }

        private static bool InvokeWithTimeout(Action act, TimeSpan timeout, ref IAsyncResult result)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            result = result ?? act.BeginInvoke(null,null);
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


        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void ViewResultsMatchDefinition(RemoteView view)
        {
            var countMaxExpectedValues = CountMaxExpectedValues(view);

            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                int cycles = 1;

                foreach (var viewComputationResultModel in resultsEnum)
                {
                    foreach (var viewResultEntry in viewComputationResultModel.AllResults)
                    {
                        Assert.NotNull(viewResultEntry.ComputedValue.Value);
                        AssertDefinitionContains(view, viewResultEntry);
                    }

                    
                    var countActualValues = viewComputationResultModel.AllResults.Count();
                        
                    Console.Out.WriteLine("{0} {1} {2}",view.Name, countActualValues, countMaxExpectedValues);
                    Assert.InRange(countActualValues, 1, countMaxExpectedValues);

                    if (--cycles <=0) 
                        return;
                }

            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void ViewResultsHaveSaneValues(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                foreach (var viewComputationResultModel in resultsEnum)
                {
                    foreach (var viewResultEntry in viewComputationResultModel.AllResults)
                    {
                        ValueAssertions.AssertSensibleValue(viewResultEntry.ComputedValue.Value);
                    }
                    break;
                }

            }
        }
        
        private static void AssertDefinitionContains(RemoteView view, ViewResultEntry viewResultEntry)
        {
            var configuration = view.Definition.CalculationConfigurationsByName[viewResultEntry.CalculationConfiguration];

            var valueSpecification = viewResultEntry.ComputedValue.Specification;

            foreach (var req in configuration.SpecificRequirements)
            {
                bool matches = (req.TargetSpecification.Uid ==
                                valueSpecification.TargetSpecification.Uid
                                && req.TargetSpecification.Type == valueSpecification.TargetSpecification.Type
                                && req.ValueName == valueSpecification.ValueName);
                //TODO constraints?
                if (matches)
                    return;

            }
            
            var reqsByType= configuration.PortfolioRequirementsBySecurityType;
            foreach (var valuePropertiese in reqsByType)
            {
                var hashSet = valuePropertiese.Value.Properties["portfolioRequirement"];
                if (!hashSet.Contains(valueSpecification.ValueName))
                {
                    Assert.True(false, string.Format("Unmatched requirement {0},{1},{2} on {3}", valueSpecification.ValueName,
                                                     valueSpecification.TargetSpecification.Type,
                                                     valueSpecification.TargetSpecification.Uid, view.Name));
                }
            }
        }

        private static int CountMaxExpectedValues(RemoteView view)
        {
            var specifics = view.Definition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.SpecificRequirements.Count());
            int rows = CountRows(view.Portfolio);
            var values = rows * view.Definition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.PortfolioRequirementsBySecurityType.Values.Sum(kvp2 => kvp2.Properties["portfolioRequirement"].Count));
            return specifics
                + values
                ;
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
