//-----------------------------------------------------------------------
// <copyright file="ICompiledViewDefinitionWithGraphs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.DepGraph;

namespace OGDotNet.Mappedtypes.Engine.View.compilation
{
    public interface ICompiledViewDefinitionWithGraphs : ICompiledViewDefinition
    {
        IDependencyGraphExplorer GetDependencyGraphExplorer(string calcConfig);
    }
}