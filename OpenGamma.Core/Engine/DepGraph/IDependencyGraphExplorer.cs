// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDependencyGraphExplorer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.Value;

namespace OpenGamma.Engine.DepGraph
{
    public interface IDependencyGraphExplorer
    {
        IDependencyGraph GetWholeGraph();
        IDependencyGraph GetSubgraphProducing(ValueSpecification output);
    }
}
