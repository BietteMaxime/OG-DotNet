//-----------------------------------------------------------------------
// <copyright file="DependencyGraph.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.depGraph;

namespace OGDotNet.Mappedtypes.engine.depgraph
{
    [FudgeSurrogate(typeof(DependencyGraphBuilder))]
    public class DependencyGraph : IDependencyGraph
    {
        private readonly string _calcConfigName;
        private readonly ICollection<DependencyNode> _dependencyNodes = new List<DependencyNode>();

        public DependencyGraph(string calcConfigName, List<DependencyNode> nodes)
        {
            _calcConfigName = calcConfigName;
            _dependencyNodes = nodes;
        }

        public string CalculationConfigurationName
        {
            get { return _calcConfigName; }
        }

        public ICollection<DependencyNode> DependencyNodes
        {
            get { return _dependencyNodes; }
        }
    }
}
