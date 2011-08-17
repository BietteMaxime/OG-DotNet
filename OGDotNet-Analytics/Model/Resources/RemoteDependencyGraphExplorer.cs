//-----------------------------------------------------------------------
// <copyright file="RemoteDependencyGraphExplorer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Apache.NMS;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Model.Resources
{
    internal class RemoteDependencyGraphExplorer : IDependencyGraphExplorer
    {
        private readonly RestTarget _resolve;

        public RemoteDependencyGraphExplorer(RestTarget resolve)
        {
            _resolve = resolve;
        }

        public IDependencyGraph GetWholeGraph()
        {
            var dependencyGraph = _resolve.Resolve("wholeGraph").Get<IDependencyGraph>();
            if (dependencyGraph == null)
            {
                throw new IllegalStateException("Null graph returned, perhaps the cycle dissapeared");
            }

            return dependencyGraph;
        }

        public IDependencyGraph GetSubgraphProducing(ValueSpecification output)
        {
            string encodedValueSpec = _resolve.EncodeBean(output);
            var subGraphTarget = _resolve.Resolve("subgraphProducing", Tuple.Create("msg", encodedValueSpec));

            var dependencyGraph = subGraphTarget.Get<IDependencyGraph>();
            if (dependencyGraph == null)
            {
                throw new IllegalStateException("Null graph returned, perhaps the cycle dissapeared");
            }
            return dependencyGraph;
        }
    }
}