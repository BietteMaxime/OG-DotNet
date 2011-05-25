//-----------------------------------------------------------------------
// <copyright file="IDependencyGraph.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.depgraph;

namespace OGDotNet.Mappedtypes.engine.depGraph
{
    public interface IDependencyGraph
    {
        string CalculationConfigurationName { get; }
        ICollection<DependencyNode> DependencyNodes { get; }
    }
}
