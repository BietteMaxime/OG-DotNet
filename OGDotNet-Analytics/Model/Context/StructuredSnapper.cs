//-----------------------------------------------------------------------
// <copyright file="StructuredSnapper.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;

namespace OGDotNet.Model.Context
{
    internal abstract class StructuredSnapper
    {
        private readonly string _requirementName;

        protected StructuredSnapper(string requirementName)
        {
            _requirementName = requirementName;
        }

        public string RequirementName
        {
            get { return _requirementName; }
        }

        protected static Dictionary<string, IEnumerable<ValueSpecification>> GetMatchingSpecifications(IDictionary<string, IDependencyGraph> graphs, string specName)
        {
            var ret = new Dictionary<string, IEnumerable<ValueSpecification>>();

            foreach (var kvp in graphs)
            {
                var config = kvp.Key;
                var graph = kvp.Value;

                var specs = graph.DependencyNodes.SelectMany(n => n.OutputValues.Where(v => v.ValueName == specName));
                ret.Add(config, specs.ToList());
            }
            return ret;
        }
    }

    internal class StructuredSnapper<TKey, TCalculatedValue, TSnapshot> : StructuredSnapper
    {
        private readonly Func<ValueSpecification, TKey> _keyProjecter;
        private readonly Func<RemoteEngineContext, IViewComputationResultModel, TKey, TCalculatedValue, TSnapshot> _snapshotProjecter;

        public StructuredSnapper(string requirementName, Func<ValueSpecification, TKey> keyProjecter, Func<RemoteEngineContext, IViewComputationResultModel, TKey, TCalculatedValue, TSnapshot> snapshotProjecter)
            : base(requirementName)
        {
            _keyProjecter = keyProjecter;
            _snapshotProjecter = snapshotProjecter;
        }

        public Dictionary<TKey, TSnapshot> GetValues(IViewComputationResultModel results, IDictionary<string, IDependencyGraph> graphs, IViewCycle viewCycle, RemoteEngineContext remoteEngineContext)
        {
            var calculatedValues = GetValues(viewCycle, graphs);
            return calculatedValues.ToDictionary(k => k.Key, k => _snapshotProjecter(remoteEngineContext, results, k.Key, k.Value));
        }

        private Dictionary<TKey, TCalculatedValue> GetValues(IViewCycle viewCycle, IDictionary<string, IDependencyGraph> dependencyGraphs)
        {
            var values = GetMatchingSpecifications(dependencyGraphs, RequirementName);
            var ts = new Dictionary<TKey, TCalculatedValue>();

            foreach (var value in values)
            {
                var requiredSpecs = value.Value.Where(r => !ts.ContainsKey(_keyProjecter(r))).ToList();
                if (!requiredSpecs.Any())
                {
                    continue;
                }
                var computationCacheResponse =
                    viewCycle.QueryComputationCaches(new ComputationCacheQuery(value.Key, requiredSpecs));

                if (computationCacheResponse.Results.Count != requiredSpecs.Count())
                {
                    //TODO LOG throw new ArgumentException("Failed to get all results");
                }

                var infos = computationCacheResponse.Results.ToDictionary(r => _keyProjecter(r.First));
                foreach (var result in infos)
                {
                    ts.Add(result.Key, (TCalculatedValue) result.Value.Second);
                }
            }
            return ts;
        }
    }
}