﻿//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycleTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Engine.View.Listener;
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
               Assert.InRange(cycle.GetDurationNanos(), 1, long.MaxValue);
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

        [Xunit.Extensions.Fact]
        public void CanHoldLotsOfCycles()
        {
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                const int cyclesCount = 1000;

                var cycles = new BlockingCollection<IEngineResourceReference<IViewCycle>>();

                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { cycles.Add(null); };
                listener.ViewDefinitionCompilationFailed += delegate { cycles.Add(null); };
                listener.CycleExecutionFailed += delegate { cycles.Add(null); };

                listener.CycleCompleted += (sender, e) =>
                                               {
                                                   remoteViewClient.Pause();
                                                   //Use parallel to fill the thread pool
                                                   Parallel.For(0, cyclesCount, _ => cycles.Add(remoteViewClient.CreateCycleReference(e.FullResult.ViewCycleId)));
                                               };

                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);
                var options = ExecutionOptions.RealTime;
                remoteViewClient.AttachToViewProcess("Demo Equity Option Test View", options);

                var cyclesList = new List<IEngineResourceReference<IViewCycle>>();

                TimeSpan timeout = TimeSpan.FromMinutes(2);
                for (int i = 0; i < cyclesCount; i++)
                {
                    IEngineResourceReference<IViewCycle> cycle;
                    if (!cycles.TryTake(out cycle, timeout))
                    {
                        throw new TimeoutException(string.Format("Failed to get result {0} in {1}", i, timeout));
                    }
                    if (cycle == null)
                    {
                        throw new Exception("Some error occured");
                    }
                    cyclesList.Add(cycle);
                }
                remoteViewClient.DetachFromViewProcess(); //Stop gathering more and more referencess

                Thread.Sleep(TimeSpan.FromSeconds(20)); //Make sure we have to heartbeat a few times
                foreach (var engineResourceReference in cyclesList)
                {
                    Assert.NotNull(engineResourceReference.Value.UniqueId);
                }
            }
        }

        [Theory(Parallel = false)]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void NumberOfResultsIsConsistent(ViewDefinition defn)
        {
            const int cyclesCount = 5;

            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            using (var mre = new ManualResetEvent(false))
            {
                var cycles = new BlockingCollection<IEngineResourceReference<IViewCycle>>();

                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { mre.Set(); };
                listener.ViewDefinitionCompilationFailed += delegate { mre.Set(); };
                listener.CycleExecutionFailed += delegate { mre.Set(); };

                listener.CycleCompleted += (sender, e) =>
                                               {
                                                   cycles.Add(remoteViewClient.CreateCycleReference(e.FullResult.ViewCycleId));
                                                   remoteViewClient.TriggerCycle();
                                               };

                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);

                var sequence = ArbitraryViewCycleExecutionSequence.Create(Enumerable.Range(0, cyclesCount).Select(i => DateTimeOffset.Now + TimeSpan.FromHours(i)));
                var options = new ExecutionOptions(sequence, ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, null, new ViewCycleExecutionOptions(default(DateTimeOffset), ExecutionOptions.GetDefaultMarketDataSpec()));

                remoteViewClient.AttachToViewProcess(defn.UniqueID, options);
                
                TimeSpan timeout = TimeSpan.FromMinutes(5);
                if (! mre.WaitOne(timeout))
                {
                    throw new TimeoutException(string.Format("Failed to get result in {0}", timeout));
                }
                Assert.Equal(cyclesCount, cycles.Count);
                
                var specs = cycles.Select(GetAllSpecs).ToList();

                var inconsistent = specs.Zip(specs.Skip(1), Tuple.Create).SelectMany(
                    t =>
                        {
                            var diff = new HashSet<Tuple<string, ValueSpecification>>(t.Item1);
                            diff.SymmetricExceptWith(t.Item2);
                            return diff;
                        }).Distinct();
                if (inconsistent.Any())
                {
                    var counts = string.Join(",", specs.Select(c => c.Count.ToString()));
                    var inconsistentStrings = specs.Select(s => string.Join(",", s.Where(x => inconsistent.Contains(x)).Select(x => x.ToString())));
                    string inconsistentString = string.Join(Environment.NewLine, inconsistentStrings);
                    throw new Exception(string.Format("Inconsistent number of results for {0} {1}: {2}", defn.Name, counts, inconsistentString));
                }
            }
        }

        private static HashSet<Tuple<string, ValueSpecification>> GetAllSpecs(IEngineResourceReference<IViewCycle> cycle)
        {
            using (cycle)
            {
                return GetAllSpecs(cycle.Value);
            }
        }

        private static HashSet<Tuple<string, ValueSpecification>> GetAllSpecs(IViewCycle cycle)
        {
            var specSet = new HashSet<Tuple<string, ValueSpecification>>();

            var compiledViewDefinition = cycle.GetCompiledViewDefinition();
            foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
            {
                var viewCalculationConfiguration = kvp.Key;
   
                var dependencyGraphExplorer = compiledViewDefinition.GetDependencyGraphExplorer(viewCalculationConfiguration);
                Assert.NotNull(dependencyGraphExplorer);
                var wholeGraph = dependencyGraphExplorer.GetWholeGraph();

                IEnumerable<ValueSpecification> allSpecs = wholeGraph.DependencyNodes.SelectMany(n => n.OutputValues);
                var distinctKindsOfSpec = allSpecs
                    .ToLookup(s => s.ValueName).Select(g => g.First());
                var specs = new HashSet<ValueSpecification>(distinctKindsOfSpec);

                if (!specs.Any())
                {
                    continue;
                }
                var computationCacheResponse = cycle.QueryComputationCaches(new ComputationCacheQuery(viewCalculationConfiguration, specs));
                Assert.InRange(computationCacheResponse.Results.Count, 0, specs.Count());
                foreach (var result in computationCacheResponse.Results)
                {
                    Assert.Contains(result.First, specs);
                    Assert.NotNull(result.Second);
                    ValueAssertions.AssertSensibleValue(result.Second);
                }
                var newSpecs = computationCacheResponse.Results.Select(p => Tuple.Create(viewCalculationConfiguration, p.First));
                foreach (var newSpec in newSpecs)
                {
                    Assert.True(specSet.Add(newSpec));
                }
            }

            return specSet;
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

                    if (! specs.Any())
                    {
                        continue;
                    }
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

        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetAllTerminalValues(ViewDefinition defn)
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                var viewComputationResultModel = cycle.GetResultModel();
                foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
                {
                    var viewCalculationConfiguration = kvp.Key;
                    var results = viewComputationResultModel.AllResults.Where(r => r.CalculationConfiguration == viewCalculationConfiguration).ToDictionary(r => r.ComputedValue.Specification, r => r.ComputedValue.Value);
                    var specs = results.Select(r => r.Key).ToList();

                    var computationCacheResponse = cycle.QueryComputationCaches(new ComputationCacheQuery(viewCalculationConfiguration, specs));
                    Assert.Equal(specs.Count(), computationCacheResponse.Results.Count);
                    foreach (var result in computationCacheResponse.Results)
                    {
                        Assert.Contains(result.First, specs);
                        var expected = results[result.First];
                        Assert.Equal(expected.GetType(), result.Second.GetType());
                        if (expected is double)
                        {
                            Assert.Equal(expected, result.Second);
                        }
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
                        string.Join(",", dependencyNode.OutputValues.Select(v => v.ValueName).Distinct()), shape));
                }

                for (int index = 0; index < dependencyNodes.Count; index++)
                {
                    var dependencyNode = dependencyNodes[index];
                    foreach (var inputNode in dependencyNode.InputNodes)
                    {
                        var labels = inputNode.OutputValues.Intersect(dependencyNode.InputValues).Select(v => v.ValueName).Distinct();
                        var edgelabel = string.Join(",", labels);
                        streamWriter.WriteLine(string.Format("{0} -> {1} [label=\"{2}\"];", map[inputNode], index, edgelabel));
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

        public static void WithViewCycle(Action<ViewDefinitionCompiledArgs, IViewCycle, RemoteViewClient> action, string viewName = "Demo Equity Option Test View")
        {
            using (var executedMre = new ManualResetEventSlim(false))
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                ViewDefinitionCompiledArgs compiled = null;
                CycleCompletedArgs cycle = null;
                JavaException error = null;
                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { executedMre.Set(); };
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                                               {
                                                   cycle = e;
                                                   executedMre.Set();
                                               };
                listener.ViewDefinitionCompiled += delegate(object sender, ViewDefinitionCompiledArgs e) { compiled = e; };
                listener.ViewDefinitionCompilationFailed += delegate(object sender, ViewDefinitionCompilationFailedArgs e)
                                                                {
                                                                    error = e.Exception; 
                                                                    executedMre.Set(); 
                                                                };

                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);
                remoteViewClient.AttachToViewProcess(viewName, ExecutionOptions.SingleCycle);
                Assert.Null(remoteViewClient.CreateLatestCycleReference());

                executedMre.Wait(TimeSpan.FromMinutes(1));
                Assert.Null(error);
                Assert.NotNull(compiled);
                Assert.NotNull(cycle);

                using (var engineResourceReference = remoteViewClient.CreateLatestCycleReference())
                {
                    action(compiled, engineResourceReference.Value, remoteViewClient);
                }
            }
        }
    }
}