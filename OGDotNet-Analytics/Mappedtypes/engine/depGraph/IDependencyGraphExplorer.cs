//-----------------------------------------------------------------------
// <copyright file="IDependencyGraphExplorer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.depGraph
{
    public interface IDependencyGraphExplorer
    {
        IDependencyGraph GetSubgraphProducing(ValueSpecification output);
    }
}
