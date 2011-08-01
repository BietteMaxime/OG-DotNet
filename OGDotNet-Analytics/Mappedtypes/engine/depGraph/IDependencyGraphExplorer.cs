//-----------------------------------------------------------------------
// <copyright file="IDependencyGraphExplorer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.value;

namespace OGDotNet.Mappedtypes.Engine.DepGraph
{
    public interface IDependencyGraphExplorer
    {
        IDependencyGraph GetWholeGraph();
        IDependencyGraph GetSubgraphProducing(ValueSpecification output);
    }
}
