//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.tuple;
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
            return PagedQuery(computationCacheQuery.CalculationConfigurationName, computationCacheQuery.ValueSpecifications);
        }

        const int MaxQuerySize = 5; // TODO PLAT-1304
        private ComputationCacheResponse PagedQuery(string config, IEnumerable<ValueSpecification> specs)
        {
            var pages = specs.Select((s, i) => new { Page = i % MaxQuerySize, Spec = s }).GroupBy(t => t.Page).Select(g => g.Select(p => p.Spec));
            return pages.Select(p => QueryComputationCacheImpl(new ComputationCacheQuery(config, p))).Aggregate(new ComputationCacheResponse(new List<Pair<ValueSpecification, object>>()), Join);
        }

        private ComputationCacheResponse QueryComputationCacheImpl(ComputationCacheQuery computationCacheQuery)
        {
            string msgBase64 = _location.EncodeBean(computationCacheQuery);
            return _location.Resolve("queryCaches", new Tuple<string, string>("msg", msgBase64)).Get<ComputationCacheResponse>() ?? new ComputationCacheResponse(new List<Pair<ValueSpecification, object>>());
        }

        private static ComputationCacheResponse Join(ComputationCacheResponse tail, ComputationCacheResponse head)
        {
            return new ComputationCacheResponse(tail.Results.Concat(head.Results).ToList());
        }

        public UniqueIdentifier GetViewProcessId()
        {
            return _location.Resolve("viewProcessId").Get<UniqueIdentifier>();
        }

        public ViewCycleState GetState()
        {
            var fudgeMsg = _location.Resolve("state").GetFudge();
            return EnumBuilder<ViewCycleState>.Parse(fudgeMsg.GetString(1));
        }

        public long GetDurationNanos()
        {
            var fudgeMsg = _location.Resolve("duration").GetFudge();
            return fudgeMsg.GetLong("value").Value;
        }
    }
}