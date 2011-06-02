//-----------------------------------------------------------------------
// <copyright file="RemoteDependencyGraphExplorer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;

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
            return _resolve.Resolve("wholeGraph").Get<IDependencyGraph>();
        }

        public IDependencyGraph GetSubgraphProducing(ValueSpecification output)
        {
            string encodedValueSpec = _resolve.EncodeBean(output);
            var subGraphTarget = _resolve.Resolve("subgraphProducing", Tuple.Create("msg", encodedValueSpec));

            return subGraphTarget.Get<IDependencyGraph>();
        }
    }
}