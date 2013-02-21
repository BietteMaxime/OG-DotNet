// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompiledViewDefinitionWithGraphs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.DepGraph;

namespace OpenGamma.Engine.View.Compilation
{
    public interface ICompiledViewDefinitionWithGraphs : ICompiledViewDefinition
    {
        IDependencyGraphExplorer GetDependencyGraphExplorer(string calcConfig);
    }
}