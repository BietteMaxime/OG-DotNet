//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycleTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Model.Resources;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewCycleTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetCycle()
        {
            WithViewCycle(
           delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
           {
               Assert.NotNull(cycle.UniqueId);
               var resultModel = cycle.GetResultModel();
               Assert.NotNull(resultModel);

               var computedValue = resultModel.AllResults.First().ComputedValue;
               var valueSpec = computedValue.Specification;

               var nonEmptyResponse = cycle.QueryComputationCaches(new ComputationCacheQuery("Default", valueSpec));

               Assert.NotNull(nonEmptyResponse);

               var results = nonEmptyResponse.Results;
               Assert.NotEmpty(results);
               Assert.Equal(1, results.Count());
               Assert.Equal(computedValue.Specification, results.Single().First);
               Assert.Equal(computedValue.Value, results.Single().Second);

               Assert.NotNull(cycle.GetViewProcessId());
               Assert.Equal(ViewCycleState.Executed, cycle.GetState());
               var duration = cycle.GetDurationNanos();
               Assert.InRange(duration, 10, long.MaxValue);
           });
        }

        [Xunit.Extensions.Fact]
        public void CantDoStupidCacheQuery()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                Assert.Throws<ArgumentException>(() => cycle.QueryComputationCaches(new ComputationCacheQuery("Default")));
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetCycleById()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                using (
                    var refById = client.CreateCycleReference(cycle.UniqueId))
                {
                    Assert.Equal(refById.Value.UniqueId, cycle.UniqueId);
                }
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetViewDefintion()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                Assert.NotNull(compiledViewDefinition.ViewDefinition);
                Assert.NotEmpty(compiledViewDefinition.CompiledCalculationConfigurations);
                Assert.Equal(compiled.CompiledViewDefinition.CompiledCalculationConfigurations.Keys, compiledViewDefinition.CompiledCalculationConfigurations.Keys);

                Assert.Equal(compiled.CompiledViewDefinition.EarliestValidity, compiledViewDefinition.EarliestValidity);
                Assert.Equal(compiled.CompiledViewDefinition.LatestValidity, compiledViewDefinition.LatestValidity);

                Assert.NotEmpty(compiledViewDefinition.LiveDataRequirements);
                Assert.Equal(compiled.CompiledViewDefinition.LiveDataRequirements.Count, compiledViewDefinition.LiveDataRequirements.Count);

                Assert.NotNull(compiledViewDefinition.Portfolio);
                Assert.Equal(compiled.CompiledViewDefinition.Portfolio.UniqueId, compiledViewDefinition.Portfolio.UniqueId);
            });
        }

        [Xunit.Extensions.Fact]
        public void CycleStaysAlive()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                Thread.Sleep(10000);
                Assert.Equal(ViewCycleState.Executed, cycle.GetState());
            });
        }

        private static void WithViewCycle(Action<ViewDefinitionCompiledArgs, IViewCycle, RemoteViewClient> action)
        {
            using (var executedMre = new ManualResetEventSlim(false))
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                ViewDefinitionCompiledArgs compiled = null;

                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { executedMre.Set(); };
                listener.ViewDefinitionCompiled += delegate(object sender, ViewDefinitionCompiledArgs e) { compiled = e; };

                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", ExecutionOptions.SingleCycle);
                Assert.Null(remoteViewClient.CreateLatestCycleReference());

                executedMre.Wait(TimeSpan.FromMinutes(1));
                Assert.NotNull(compiled);

                using (var engineResourceReference = remoteViewClient.CreateLatestCycleReference())
                {
                    action(compiled, engineResourceReference.Value, remoteViewClient);
                }
            }
        }
    }
}