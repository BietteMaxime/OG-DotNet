//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycleTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
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

               var computedValue = resultModel.AllResults.First(r => r.ComputedValue.Value is double).ComputedValue;
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

                Assert.NotEmpty(compiledViewDefinition.MarketDataRequirements);
                Assert.Equal(compiled.CompiledViewDefinition.MarketDataRequirements.Count, compiledViewDefinition.MarketDataRequirements.Count);

                Assert.NotNull(compiledViewDefinition.Portfolio);
                Assert.Equal(compiled.CompiledViewDefinition.Portfolio.UniqueId, compiledViewDefinition.Portfolio.UniqueId);
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetSubGraphs()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                var resultModel = cycle.GetResultModel();
                foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
                {
                    var viewCalculationConfiguration = kvp.Key;
                    var dependencyGraphExplorer = compiledViewDefinition.GetDependencyGraphExplorer(viewCalculationConfiguration);

                    var vresToTest = resultModel.AllResults.Where(r => r.CalculationConfiguration == viewCalculationConfiguration);
                    foreach (var vreToTest in vresToTest)
                    {
                        var specToTest = vreToTest.ComputedValue.Specification;

                        Assert.NotNull(dependencyGraphExplorer);
                        var subgraph = dependencyGraphExplorer.GetSubgraphProducing(specToTest);
                        CheckSaneGraph(viewCalculationConfiguration, subgraph);

                        Assert.True(subgraph.DependencyNodes.Any(n => Produces(n, specToTest)));

                        var lastNode = subgraph.DependencyNodes.Single(n => Produces(n, specToTest));
                        Assert.True(lastNode.TerminalOutputValues.Contains(specToTest));

                        //Check the graph is connected
                        Assert.Equal(FollowInputs(lastNode).Count, subgraph.DependencyNodes.Count);
                    }
                }
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetWholeGraph()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
                {
                    var viewCalculationConfiguration = kvp.Key;

                    var dependencyGraphExplorer = compiledViewDefinition.GetDependencyGraphExplorer(viewCalculationConfiguration);
                    Assert.NotNull(dependencyGraphExplorer);
                    var wholeGraph = dependencyGraphExplorer.GetWholeGraph();
                    
                    CheckSaneGraph(viewCalculationConfiguration, wholeGraph);
                    CheckCompleteGraph(wholeGraph);
                }
            });
        }

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetAllKindsOfValues(ViewDefinition defn)
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
                {
                    var viewCalculationConfiguration = kvp.Key;

                    var dependencyGraphExplorer =
                        compiledViewDefinition.GetDependencyGraphExplorer(viewCalculationConfiguration);
                    Assert.NotNull(dependencyGraphExplorer);
                    var wholeGraph = dependencyGraphExplorer.GetWholeGraph();

                    var distinctKindsOfSpec = wholeGraph.DependencyNodes.SelectMany(n => n.OutputValues)
                        .ToLookup(s => s.ValueName).Select(g => g.First());
                    var specs = new HashSet<ValueSpecification>(distinctKindsOfSpec);

                    var computationCacheResponse = cycle.QueryComputationCaches(new ComputationCacheQuery(viewCalculationConfiguration, specs));
                    Assert.InRange(computationCacheResponse.Results.Count, 0, specs.Count());
                    foreach (var result in computationCacheResponse.Results)
                    {
                        Assert.Contains(result.First, specs);
                        Assert.NotNull(result.Second);
                        ValueAssertions.AssertSensibleValue(result.Second);
                    }
                }
            }, defn.Name);
        }

        private static void CheckCompleteGraph(IDependencyGraph wholeGraph)
        {
            foreach (DependencyNode node in wholeGraph.DependencyNodes)
            {
                if (! node.TerminalOutputValues.Any())
                {
                    Assert.True(wholeGraph.DependencyNodes.Any(n => n.InputNodes.Contains(node)));
                }
            }
        }

        private static void CheckSaneGraph(string viewCalculationConfiguration, IDependencyGraph subgraph)
        {
            Assert.NotNull(subgraph);

            Assert.Equal(viewCalculationConfiguration, subgraph.CalculationConfigurationName);
            Assert.NotEmpty(subgraph.DependencyNodes);

            foreach (var node in subgraph.DependencyNodes)
            {
                Assert.NotEmpty(node.OutputValues);

                Assert.NotNull(node.Function);
                Assert.NotNull(node.Function.UniqueId);
                Assert.NotNull(node.Function.Parameters);
                Assert.NotNull(node.Function.Function);
                Assert.NotNull(node.Function.Function.TargetType);
                Assert.NotNull(node.Function.Function.FunctionDefinition);
                Assert.NotEmpty(node.Function.Function.FunctionDefinition.UniqueId);
                Assert.NotEmpty(node.Function.Function.FunctionDefinition.ShortName);
            }

            WriteToDot(subgraph, string.Format("{0}.{1}.dot", viewCalculationConfiguration, TestUtils.ExecutingTestName));
        }

        private static void WriteToDot(IDependencyGraph subgraph, string dotFileName)
        {
            using (var streamWriter = new StreamWriter(dotFileName))
            {
                streamWriter.WriteLine("digraph graphname {");
                var dependencyNodes = subgraph.DependencyNodes.ToList();
                Dictionary<DependencyNode, int> map = dependencyNodes.Select((n, i) => Tuple.Create(n, i)).ToDictionary(t => t.Item1, t => t.Item2);
                for (int index = 0; index < dependencyNodes.Count; index++)
                {
                    var dependencyNode = dependencyNodes[index];
                    string shape = dependencyNode.TerminalOutputValues.Any() ? "doubleoctagon" : "ellipse";
                    streamWriter.WriteLine(string.Format("{0} [label=\"{1}\", shape=\"{2}\"];", index,
                        string.Join(",", dependencyNode.OutputValues.Select(v => string.Format("{0} - {1}", v.TargetSpecification.Uid, v.ValueName))), shape));
                }

                for (int index = 0; index < dependencyNodes.Count; index++)
                {
                    var dependencyNode = dependencyNodes[index];
                    foreach (var inputNode in dependencyNode.InputNodes)
                    {
                        streamWriter.WriteLine(string.Format("{0} -> {1} [label=\"{2}\"];", map[inputNode], index, string.Empty));
                    }
                }
                streamWriter.WriteLine("}");
            }
        }

        private static ISet<DependencyNode> FollowInputs(DependencyNode dependencyNode)
        {
            var set = new HashSet<DependencyNode>();
            FollowInputs(set, dependencyNode);
            return set;
        }

        private static void FollowInputs(HashSet<DependencyNode> nodes, DependencyNode dependencyNode)
        {
            if (nodes.Contains(dependencyNode))
                return;
            nodes.Add(dependencyNode);
            foreach (var inputNode in dependencyNode.InputNodes)
            {
                FollowInputs(nodes, inputNode);
            }
        }

        private static bool Produces(DependencyNode n, ValueSpecification specToTest)
        {
            var targetMatches = n.Target.UniqueId == specToTest.TargetSpecification.Uid && n.Target.Type == specToTest.TargetSpecification.Type;
            return targetMatches && n.OutputValues.Any(s => s.Equals(specToTest));
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

        private static void WithViewCycle(Action<ViewDefinitionCompiledArgs, IViewCycle, RemoteViewClient> action, string viewName = "Equity Option Test View 1")
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
                remoteViewClient.AttachToViewProcess(viewName, ExecutionOptions.SingleCycle);
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