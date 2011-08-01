//-----------------------------------------------------------------------
// <copyright file="RemoteViewClientBatchTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Engine.marketdata.spec;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Engine.View.listener;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientBatchTests : TestWithContextBase
    {
        public const string ViewName = "Equity Option Test View 1";

        [Xunit.Extensions.Fact]
        public void CanRunZeroCycleBatch()
        {
            var runToCompletion = RunToCompletion(ExecutionOptions.Batch(ArbitraryViewCycleExecutionSequence.Of(new DateTimeOffset[] { })));
            Assert.Empty(runToCompletion.Item1);
            Assert.Empty(runToCompletion.Item2);
        }

        [Xunit.Extensions.Fact]
        public void CanRunSingleCycleBatch()
        {
            IViewExecutionOptions req = ExecutionOptions.SingleCycle;
            var runToCompletion = RunToCompletion(req);
            Assert.Equal(1, runToCompletion.Item1.Count());
            Assert.Equal(1, runToCompletion.Item2.Count());
            AssertApproximatelyEqual(req.ExecutionSequence.Next.ValuationTime, runToCompletion.Item2.Single().FullResult.ValuationTime);
        }

        [Xunit.Extensions.Fact]
        public void CanRunSingleYesterdayCycleBatch()
        {
            IViewExecutionOptions req = ExecutionOptions.GetSingleCycle(DateTimeOffset.Now - TimeSpan.FromDays(1), new LiveMarketDataSpecification());
            var runToCompletion = RunToCompletion(req);
            Assert.Equal(1, runToCompletion.Item1.Count());
            Assert.Equal(1, runToCompletion.Item2.Count());
            AssertApproximatelyEqual(req.ExecutionSequence.Next.ValuationTime, runToCompletion.Item2.Single().FullResult.ValuationTime);
        }

        [Xunit.Extensions.Fact]
        public void CanRunManyCycleBatch()
        {
            var valuationTimes = new[]
                                     {
                                         DateTimeOffset.Now,
                                         DateTimeOffset.Now + TimeSpan.FromDays(5),
                                         DateTimeOffset.Now - TimeSpan.FromDays(5)
                                     };

            var runToCompletion = RunToCompletion(ExecutionOptions.Batch(ArbitraryViewCycleExecutionSequence.Of(valuationTimes)));

            foreach (var t in runToCompletion.Item2.Zip(valuationTimes, Tuple.Create))
            {
                DateTimeOffset expected = t.Item2;
                DateTimeOffset actual = t.Item1.FullResult.ValuationTime;

                AssertApproximatelyEqual(actual, expected);
            }
        }

        private static void AssertApproximatelyEqual(DateTimeOffset actual, DateTimeOffset expected)
        {
            Assert.InRange( actual.ToUniversalTime() - expected.ToUniversalTime(), TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(1));
        }

        public static Tuple<IEnumerable<ViewDefinitionCompiledArgs>, IEnumerable<CycleCompletedArgs>> RunToCompletion(IViewExecutionOptions options)
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                var cycles = new ConcurrentQueue<CycleCompletedArgs>();
                var compiles = new ConcurrentQueue<ViewDefinitionCompiledArgs>();

                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    var listener = new EventViewResultListener();
                    listener.ViewDefinitionCompiled += (sender, e) => compiles.Enqueue(e);
                    listener.CycleCompleted += (sender, e) => cycles.Enqueue(e);
                    listener.ProcessCompleted += (sender, e) => manualResetEvent.Set();
                    remoteViewClient.SetResultListener(listener);
                    remoteViewClient.AttachToViewProcess(ViewName, options);
                    manualResetEvent.WaitOne();
                }

                Assert.InRange(compiles.Count, cycles.Any() ? 1 : 0, cycles.Count + 1);
                Assert.True(remoteViewClient.IsCompleted);
                return new Tuple<IEnumerable<ViewDefinitionCompiledArgs>, IEnumerable<CycleCompletedArgs>>(compiles, cycles);
            }
        }
    }
}