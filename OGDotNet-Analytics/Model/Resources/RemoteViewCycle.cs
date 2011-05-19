//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteViewCycle : IViewCycle
    {
        private readonly RestTarget _location;

        public RemoteViewCycle(RestTarget location)
        {
            _location = location;
        }

        public UniqueIdentifier UniqueId
        {
            get
            {
                return _location.Resolve("id").Get<UniqueIdentifier>();
            }
        }

        public ICompiledViewDefinitionWithGraphs GetCompiledViewDefinition()
        {
            return new RemoteCompiledViewDefinitionWithGraphs(_location.Resolve("compiledViewDefinition"));
        }

        public ViewComputationResultModel GetResultModel()
        {
            return _location.Resolve("result").Get<ViewComputationResultModel>();
        }

        public ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery)
        {
            ArgumentChecker.NotNull(computationCacheQuery, "computationCacheQuery");
            string msgBase64 = _location.EncodeBean(computationCacheQuery);

            return _location.Resolve("queryCaches", new Tuple<string, string>("msg", msgBase64)).Get<ComputationCacheResponse>();
        }
    }
}