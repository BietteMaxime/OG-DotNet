//-----------------------------------------------------------------------
// <copyright file="RawMarketDataSnapper.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles getting values from the engine useful for creating snapshots
    /// <list type="table">
    /// <item>TODO: this implementation is evidence for the fact that the Snapshotting shouldn't be client</item>
    /// </list>
    /// </summary>
    internal static class RawMarketDataSnapper
    {
        private const string YieldCurveValueReqName = "YieldCurve";
        private const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        private const string MarketValueReqName = "Market_Value";
        private const string YieldCurveMarketDataReqName = "YieldCurveMarketData";

        #region create snapshot

        public static ManageableMarketDataSnapshot CreateSnapshotFromCycle(IViewComputationResultModel results, IDictionary<string, IDependencyGraph> graphs, IViewCycle viewCycle, string basisViewName)
        {
            var globalValues = GetGlobalValues(results, graphs);
            var yieldCurves = GetYieldCurveValues(viewCycle, graphs, YieldCurveMarketDataReqName).ToDictionary(yieldCurve => yieldCurve.Key, yieldCurve => GetYieldCurveSnapshot((SnapshotDataBundle) yieldCurve.Value[YieldCurveMarketDataReqName], results.ValuationTime));

            return new ManageableMarketDataSnapshot(basisViewName, globalValues, yieldCurves);
        }

        private static YieldCurveKey GetYieldCurveKey(ValueSpecification y)
        {
            return new YieldCurveKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Curve"].Single());
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(SnapshotDataBundle bundle, DateTimeOffset valuationTime)
        {
            var data = bundle.DataPoints.ToDictionary(
                s => new MarketDataValueSpecification(MarketDataValueType.Primitive, s.Key),
                s => (IDictionary<string, ValueSnapshot>)new Dictionary<string, ValueSnapshot> { { MarketValueReqName, new ValueSnapshot(s.Value) } }
            );
            var values = new ManageableUnstructuredMarketDataSnapshot(data);
            return new ManageableYieldCurveSnapshot(values, valuationTime);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetGlobalValues(IViewComputationResultModel tempResults, IDictionary<string, IDependencyGraph> graphs)
        {
            var data = tempResults.AllLiveData;
            var includedSpecs = GetIncludedGlobalSpecs(graphs);
            var includedGlobalData = data
                .Where(d => includedSpecs.Contains(Tuple.Create(d.Specification.TargetSpecification, d.Specification.ValueName)));

            var dataByTarget = includedGlobalData
                .ToLookup(r => new MarketDataValueSpecification(GetMarketType(r.Specification.TargetSpecification.Type), r.Specification.TargetSpecification.Uid));
            var dict = dataByTarget.ToDictionary(g => g.Key, GroupByValueName);

            return new ManageableUnstructuredMarketDataSnapshot(dict);
        }

        private static HashSet<Tuple<ComputationTargetSpecification, string>> GetIncludedGlobalSpecs(IDictionary<string, IDependencyGraph> graphs)
        {
            var ret = new HashSet<Tuple<ComputationTargetSpecification, string>>();
            foreach (var dependencyGraph in graphs)
            {
                var nodes = GetIncludedGlobalNodes(dependencyGraph.Value);
                
                foreach (var dependencyNode in nodes)
                {
                    foreach (var input in dependencyNode.OutputValues)
                    {
                        ret.Add(Tuple.Create(input.TargetSpecification, input.ValueName));
                    }
                }
            }
            return ret;
        }

        private static IEnumerable<DependencyNode> GetIncludedGlobalNodes(IDependencyGraph dependencyGraph)
        { // LAP-37
            return DependencyGraphWalker.GetNodesExcludingDependencies(dependencyGraph, IsYieldCurveNode);
        }

        private static bool IsYieldCurveNode(DependencyNode node)
        {
            var isYieldCurveNode = node.InputValues.Any(IsYieldCurveMarketDataSpec);
            if (isYieldCurveNode && !node.InputValues.All(IsYieldCurveMarketDataSpec))
            {
                throw new ArgumentException(string.Format("Unsure how to handle node {0}", node));
            }
            return isYieldCurveNode;
        }

        private static bool IsYieldCurveMarketDataSpec(ValueSpecification s)
        {
            return s.ValueName == YieldCurveMarketDataReqName;
        }

        private static IDictionary<string, ValueSnapshot> GroupByValueName(IEnumerable<ComputedValue> r)
        {
            return r.ToLookup(e => e.Specification.ValueName)
                .ToDictionary(g => g.Key, g => new ValueSnapshot(g.Select(cv => cv.Value).Cast<double>().Distinct().Single()));
        }

        private static MarketDataValueType GetMarketType(ComputationTargetType type)
        {
            return EnumUtils<ComputationTargetType, MarketDataValueType>.ConvertTo(type);
        }
        #endregion

        public static Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> EvaluateYieldCurves(IViewCycle c, IDictionary<string, IDependencyGraph> graphs)
        {
            return GetYieldCurveValues(c, graphs, YieldCurveValueReqName, YieldCurveSpecValueReqName)
                .ToDictionary(k => k.Key, k => GetEvaluatedCurve(k.Value));
        }

        private static Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetEvaluatedCurve(Dictionary<string, object> values)
        {
            return Tuple.Create((YieldCurve)values[YieldCurveValueReqName],
                (InterpolatedYieldCurveSpecificationWithSecurities) values[YieldCurveSpecValueReqName]);
        }

        private static Dictionary<YieldCurveKey, Dictionary<string, object>> GetYieldCurveValues(IViewCycle viewCycle, IDictionary<string, IDependencyGraph> dependencyGraphs, params string[] valueNames)
        {
            var values = GetMatchingSpecifications(dependencyGraphs, valueNames);
            var yieldCurves = new Dictionary<YieldCurveKey, Dictionary<string, object>>();

            foreach (var yieldCurveSpecReq in values)
            {
                var requiredSpecs = yieldCurveSpecReq.Value.Where(r => !yieldCurves.ContainsKey(GetYieldCurveKey(r)));
                if (!requiredSpecs.Any())
                {
                    continue;
                }
                var computationCacheResponse =
                    viewCycle.QueryComputationCaches(new ComputationCacheQuery(yieldCurveSpecReq.Key, requiredSpecs));

                if (computationCacheResponse.Results.Count != requiredSpecs.Count())
                {
                    //TODO LOG throw new ArgumentException("Failed to get all results");
                }

                var yieldCurveInfo = computationCacheResponse.Results.ToLookup(r => GetYieldCurveKey(r.First));
                foreach (var result in yieldCurveInfo)
                {
                    yieldCurves.Add(result.Key, result.ToDictionary(r => r.First.ValueName, r => r.Second));
                }
            }

            return yieldCurves;
        }

        private static Dictionary<string, IEnumerable<ValueSpecification>> GetMatchingSpecifications(IDictionary<string, IDependencyGraph> graphs, params string[] specNames)
        {
            var ret = new Dictionary<string, IEnumerable<ValueSpecification>>();

            foreach (var kvp in graphs)
            {
                var config = kvp.Key;
                var graph = kvp.Value;

                var specs = graph.DependencyNodes.SelectMany(n => n.OutputValues)
                    //This is a hack: the spec value will be pruned from the dep graph but will be in the computation cache,
                    //  and we know that it has exactly the some properties as the Curve value
                    .SelectMany(v => v.ValueName == YieldCurveValueReqName ? new[] { v, new ValueSpecification(YieldCurveSpecValueReqName, v.TargetSpecification, v.Properties) } : new[] { v })

                    .Where(v => specNames.Contains(v.ValueName));
                ret.Add(config, specs.ToList());
            }

            return ret;
        }

        public static Dictionary<string, IDependencyGraph> GetGraphs(ICompiledViewDefinitionWithGraphs compiledViewDefinitionWithGraphs)
        {
            return compiledViewDefinitionWithGraphs.CompiledCalculationConfigurations.Keys
                .ToDictionary(k => k, k => compiledViewDefinitionWithGraphs.GetDependencyGraphExplorer(k).GetWholeGraph());
        }
    }
}