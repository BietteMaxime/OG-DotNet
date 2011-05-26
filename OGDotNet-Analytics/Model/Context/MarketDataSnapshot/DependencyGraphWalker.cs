//-----------------------------------------------------------------------
// <copyright file="DependencyGraphWalker.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public static class DependencyGraphWalker
    {
        /// <summary>
        /// Filters a graph according to <paramref name="nodesToExcludeWithDependencies"/>
        /// </summary>
        /// <param name="graph">The graph to select from</param>
        /// <param name="nodesToExcludeWithDependencies">These nodes and all nodes which are only in the graph as depndencies of these nodes will be excluded</param>
        /// <returns>See <paramref name="nodesToExcludeWithDependencies"/></returns>
        public static IEnumerable<DependencyNode> GetNodesExcludingDependencies(IDependencyGraph graph, Predicate<DependencyNode> nodesToExcludeWithDependencies)
        {
            var ret = new HashSet<DependencyNode>();
            foreach (var dependencyNode in graph.DependencyNodes)
            {
                if (dependencyNode.TerminalOutputValues.Any())
                {
                    AddAllDependencies(dependencyNode, ret, nodesToExcludeWithDependencies);
                }
            }
            return ret;
        }

        private static void AddAllDependencies(DependencyNode dependencyNode, HashSet<DependencyNode> dependencyNodes, Predicate<DependencyNode> nodesToExcludeWithDependencies)
        {
            if (nodesToExcludeWithDependencies(dependencyNode))
            {
                return;
            }
            if (!dependencyNodes.Add(dependencyNode))
            {
                return;
            }
            foreach (var inputNode in dependencyNode.InputNodes)
            {
                AddAllDependencies(inputNode, dependencyNodes, nodesToExcludeWithDependencies);
            }
        }
    }
}
