//-----------------------------------------------------------------------
// <copyright file="IDependencyGraph.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.DepGraph;

namespace OGDotNet.Mappedtypes.Engine.DepGraph
{
    public interface IDependencyGraph
    {
        string CalculationConfigurationName { get; }
        ICollection<DependencyNode> DependencyNodes { get; }
    }
}
