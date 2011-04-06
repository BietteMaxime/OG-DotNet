using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.util.PublicAPI;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Utils;
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
        public void CanGetUid(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                Assert.NotNull(remoteViewClient.GetUniqueId());
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetState(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var viewClientState = remoteViewClient.GetState();
                Assert.Equal(ViewClientState.Stopped, viewClientState);

                remoteViewClient.Start();
                viewClientState = remoteViewClient.GetState();
                Assert.Equal(ViewClientState.Started, viewClientState);

                remoteViewClient.Pause();
                viewClientState = remoteViewClient.GetState();
                Assert.Equal(ViewClientState.Paused, viewClientState);
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanStartAndGetAResult(RemoteView view)
        {
            Assert.NotNull(GetOneResultCache.Get(view.Name));
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanRunOneCycle(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var viewComputationResultModel = remoteViewClient.RunOneCycle(1000L);
                Assert.NotNull(viewComputationResultModel);
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanRunOneCycleByDate(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var valuationTime = DateTimeOffset.Now;
                var viewComputationResultModel = remoteViewClient.RunOneCycle(valuationTime);
                Assert.NotNull(viewComputationResultModel);

                //Now has a higher resolution
                Assert.InRange(valuationTime - viewComputationResultModel.ValuationTime.ToDateTimeOffset(),
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(1)
                    );
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanRunOneCycleByFutureDate(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var valuationTime = DateTimeOffset.Now + TimeSpan.FromDays(2);
                var viewComputationResultModel = remoteViewClient.RunOneCycle(valuationTime);
                Assert.NotNull(viewComputationResultModel);

                //Now has a higher resolution
                TimeSpan precision = TimeSpan.FromMilliseconds(1);
                Assert.InRange(valuationTime - viewComputationResultModel.ValuationTime.ToDateTimeOffset(),
                    TimeSpan.Zero-precision,
                    precision
                    );
            }
        }

        [Theory]
        [TypedPropertyData("FastTickingViews")]
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

                    for (int i = 0; i < 5; i++)
                    {
                        var requested= DateTimeOffset.Now;
                        enumerator.MoveNext();
                        Assert.NotNull(enumerator.Current);

                        
                        var valuation = enumerator.Current.ValuationTime.ToDateTimeOffset();
                        var result = enumerator.Current.ResultTimestamp.ToDateTimeOffset();
                        var now = DateTimeOffset.Now;

                        //Make sure we're not being shown up too much by the server
                        var timeToReceive = now - requested;
                        var timeToCalculate = result - valuation;
                        Assert.InRange(timeToReceive, TimeSpan.Zero, TimeSpan.FromMilliseconds(timeToCalculate.TotalMilliseconds * 3.0));
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

            var timeout = TimeSpan.FromMilliseconds(20000);

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


        readonly static Memoizer<string, ViewComputationResultModel> GetOneResultCache = new Memoizer<string, ViewComputationResultModel>(GetOneResult);
        private static ViewComputationResultModel GetOneResult(string viewName)
        {
            var view = Context.ViewProcessor.GetView(viewName);

            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                foreach (var viewComputationResultModel in resultsEnum)
                {
                    return viewComputationResultModel;
                }
            }
            throw new Exception();
        }

        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void ViewResultsMatchDefinition(RemoteView view)
        {
            var countMaxExpectedValues = CountMaxExpectedValues(view);

            var viewComputationResultModel = GetOneResultCache.Get(view.Name);

            foreach (var viewResultEntry in viewComputationResultModel.AllResults)
            {
                Assert.NotNull(viewResultEntry.ComputedValue.Value);
                AssertDefinitionContains(view, viewResultEntry);
            }


            var countActualValues = viewComputationResultModel.AllResults.Count();

            Console.Out.WriteLine("{0} {1} {2}", view.Name, countActualValues, countMaxExpectedValues);
            Assert.InRange(countActualValues, 1, countMaxExpectedValues);
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void ViewResultsHaveSaneValues(RemoteView view)
        {
            var viewComputationResultModel = GetOneResultCache.Get(view.Name);
            
            foreach (var viewResultEntry in viewComputationResultModel.AllResults)
            {
                ValueAssertions.AssertSensibleValue(viewResultEntry.ComputedValue.Value);
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
                                && req.ValueName == valueSpecification.ValueName)
                                && req.Constraints.IsSatisfiedBy(valueSpecification.Properties) ;

                if (matches)
                    return;

            }
            
            var reqsByType= configuration.PortfolioRequirementsBySecurityType;
            foreach (var valuePropertiese in reqsByType)
            {
                var hashSet = valuePropertiese.Value;
                if (!hashSet.Any(t => t.Item1 == valueSpecification.ValueName))
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
            var values = rows * view.Definition.CalculationConfigurationsByName.Sum(kvp => kvp.Value.PortfolioRequirementsBySecurityType.Single().Value.Count);
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
