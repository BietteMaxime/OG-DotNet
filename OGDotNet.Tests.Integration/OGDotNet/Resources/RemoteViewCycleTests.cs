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
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewCycleTests : ViewTestsBase
    {
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

        [Xunit.Extensions.Fact]
        public void CanGetCycleById()
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
                    var refById = remoteViewClient.CreateCycleReference(engineResourceReference.Value.UniqueId);
                    Assert.Equal(refById.Value.UniqueId, engineResourceReference.Value.UniqueId);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetViewDefintion()
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
                    var compiledViewDefinition = engineResourceReference.Value.GetCompiledViewDefinition();
                    Assert.NotNull(compiledViewDefinition.ViewDefinition);
                    Assert.NotEmpty(compiledViewDefinition.CompiledCalculationConfigurations);
                    Assert.Equal(compiled.CompiledViewDefinition.CompiledCalculationConfigurations.Keys, compiledViewDefinition.CompiledCalculationConfigurations.Keys);
                    
                    Assert.Equal(compiled.CompiledViewDefinition.EarliestValidity, compiledViewDefinition.EarliestValidity);
                    Assert.Equal(compiled.CompiledViewDefinition.LatestValidity, compiledViewDefinition.LatestValidity);

                    Assert.NotEmpty(compiledViewDefinition.LiveDataRequirements);
                    Assert.Equal(compiled.CompiledViewDefinition.LiveDataRequirements.Count, compiledViewDefinition.LiveDataRequirements.Count);

                    Assert.NotNull(compiledViewDefinition.Portfolio);
                    Assert.Equal(compiled.CompiledViewDefinition.Portfolio.UniqueId, compiledViewDefinition.Portfolio.UniqueId);
                    
                }
            }
        }
    }
}