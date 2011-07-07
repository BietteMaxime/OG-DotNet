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
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.math.curve;
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
        private const string YieldCurveValueReqName = ValueRequirementNames.YieldCurve;
        private const string YieldCurveSpecValueReqName = ValueRequirementNames.YieldCurveSpec;
        private const string YieldCurveInterpolatedValueReqName = ValueRequirementNames.YieldCurveInterpolated;
        private const string MarketValueReqName = ValueRequirementNames.MarketValue;
        private const string YieldCurveMarketDataReqName = ValueRequirementNames.YieldCurveMarketData;

        private const string VolatilityCubeMarketDataReqName = ValueRequirementNames.VolatilityCubeMarketData;
        
        #region create snapshot

        public static ManageableMarketDataSnapshot CreateSnapshotFromCycle(IViewComputationResultModel results, IDictionary<string, IDependencyGraph> graphs, IViewCycle viewCycle, string basisViewName, RemoteEngineContext remoteEngineContext)
        {
            var globalValues = GetGlobalValues(results, graphs);
            var yieldCurves = GetYieldCurveValues(viewCycle, graphs, YieldCurveMarketDataReqName).ToDictionary(yieldCurve => yieldCurve.Key, yieldCurve => GetYieldCurveSnapshot((SnapshotDataBundle) yieldCurve.Value[YieldCurveMarketDataReqName], results.ValuationTime));
            var volCubeDefinitions = GetVolCubeValues(viewCycle, graphs, VolatilityCubeMarketDataReqName)
                .ToDictionary(kvp => kvp.Key, kvp => GetVolCubeSnapshot((VolatilityCubeData)kvp.Value[VolatilityCubeMarketDataReqName], remoteEngineContext.VolatilityCubeDefinitionSource.GetDefinition(kvp.Key.Currency, kvp.Key.Name)));

            return new ManageableMarketDataSnapshot(basisViewName, globalValues, yieldCurves, volCubeDefinitions);
        }

        private static YieldCurveKey GetYieldCurveKey(ValueSpecification y)
        {
            return new YieldCurveKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Curve"].Single());
        }

        private static VolatilityCubeKey GetVolCubeKey(ValueSpecification y)
        {
            return new VolatilityCubeKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Cube"].Single());
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(SnapshotDataBundle bundle, DateTimeOffset valuationTime)
        {
            ManageableUnstructuredMarketDataSnapshot values = GetUnstructured(bundle);
            return new ManageableYieldCurveSnapshot(values, valuationTime);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetUnstructured(SnapshotDataBundle bundle)
        {
            var data = bundle.DataPoints.ToDictionary(
                s => new MarketDataValueSpecification(MarketDataValueType.Primitive, s.Key),
                s => (IDictionary<string, ValueSnapshot>)new Dictionary<string, ValueSnapshot> { { MarketValueReqName, new ValueSnapshot(s.Value) } }
                );
            return new ManageableUnstructuredMarketDataSnapshot(data);
        }

        private static ManageableVolatilityCubeSnapshot GetVolCubeSnapshot(VolatilityCubeData volatilityCubeData, VolatilityCubeDefinition volatilityCubeDefinition)
        {
            var ret = new ManageableVolatilityCubeSnapshot(GetUnstructured(volatilityCubeData.OtherData));
            foreach (var ycp in volatilityCubeDefinition.AllPoints)
            {
                ret.SetPoint(ycp, new ValueSnapshot(null));
            }
            foreach (var ycp in volatilityCubeData.DataPoints)
            {
                ret.SetPoint(ycp.Key, new ValueSnapshot(ycp.Value));
            }
            foreach (var strike in volatilityCubeData.Strikes)
            {
                ret.SetStrike(strike.Key, new ValueSnapshot(strike.Value));
            }
            return ret;
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
            return DependencyGraphWalker.GetNodesExcludingDependencies(dependencyGraph, IsStructuredMarketDataNode);
        }

        private static bool IsStructuredMarketDataNode(DependencyNode node)
        {
            return IsYieldCurveNode(node) || IsVolatilityCubeNode(node);
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

        private static bool IsVolatilityCubeNode(DependencyNode node)
        {
            var isVolCubeNode = node.OutputValues.Any(IsVolatilityCubeMarketDataSpec);
            if (isVolCubeNode && !node.OutputValues.All(IsVolatilityCubeMarketDataSpec))
            {
                throw new ArgumentException(string.Format("Unsure how to handle node {0}", node));
            }
            return isVolCubeNode;
        }

        private static bool IsVolatilityCubeMarketDataSpec(ValueSpecification s)
        {
            return s.ValueName == VolatilityCubeMarketDataReqName;
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
        public static IEnumerable<ValueSpecification> GetYieldCurveSpecs(IDictionary<string, IDependencyGraph> graphs)
        {
            var matchingSpecifications = GetMatchingSpecifications(graphs, YieldCurveValueReqName, YieldCurveSpecValueReqName);
            var included = matchingSpecifications.SelectMany(s => s.Value);
            var interpolated = included.Where(s => s.ValueName == YieldCurveValueReqName).Select(
                sp => new ValueSpecification(YieldCurveInterpolatedValueReqName, sp.TargetSpecification, GetCurveProperties(sp)));
            return included.Concat(interpolated);
        }

        private static ValueProperties GetCurveProperties(ValueSpecification sp)
        {
            return ValueProperties.Create(
                new Dictionary<string, ISet<string>> {{"Curve", new HashSet<string>(sp.Properties["Curve"])}},
                new HashSet<string>());
        }

        public static Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> EvaluateYieldCurves(IViewComputationResultModel results)
        {
            var ycResults = results.AllResults
                .Where(r => new[] { YieldCurveValueReqName, YieldCurveSpecValueReqName, YieldCurveInterpolatedValueReqName }.Contains(r.ComputedValue.Specification.ValueName));
            var lookup = ycResults
                .ToLookup(r => GetYieldCurveKey(r.ComputedValue.Specification));
            return lookup
                .ToDictionary(g => g.Key,
                              g => GetEvaluatedCurve(g.ToDictionary(e => e.ComputedValue.Specification.ValueName, e => e.ComputedValue.Value)));
        }

        private static Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve> GetEvaluatedCurve(Dictionary<string, object> values)
        {
            return Tuple.Create((YieldCurve)values[YieldCurveValueReqName],
                (InterpolatedYieldCurveSpecificationWithSecurities) values[YieldCurveSpecValueReqName], (NodalDoublesCurve) values[YieldCurveInterpolatedValueReqName]);
        }

        private static Dictionary<T, Dictionary<string, object>> GetGroupedValues<T>(IViewCycle viewCycle, IDictionary<string, IDependencyGraph> dependencyGraphs, Func<ValueSpecification, T> projecter, params string[] valueNames)
        {
            var values = GetMatchingSpecifications(dependencyGraphs, valueNames);
            var ts = new Dictionary<T, Dictionary<string, object>>();

            foreach (var yieldCurveSpecReq in values)
            {
                var requiredSpecs = yieldCurveSpecReq.Value.Where(r => !ts.ContainsKey(projecter(r)));
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

                var infos = computationCacheResponse.Results.ToLookup(r => projecter(r.First));
                foreach (var result in infos)
                {
                    ts.Add(result.Key, result.ToDictionary(r => r.First.ValueName, r => r.Second));
                }
            }

            return ts;
        }
        private static Dictionary<VolatilityCubeKey, Dictionary<string, object>> GetVolCubeValues(IViewCycle viewCycle, IDictionary<string, IDependencyGraph> dependencyGraphs, params string[] valueNames)
        {
            return GetGroupedValues(viewCycle, dependencyGraphs, GetVolCubeKey, valueNames);
        }
        private static Dictionary<YieldCurveKey, Dictionary<string, object>> GetYieldCurveValues(IViewCycle viewCycle, IDictionary<string, IDependencyGraph> dependencyGraphs, params string[] valueNames)
        {
            return GetGroupedValues(viewCycle, dependencyGraphs, GetYieldCurveKey, valueNames);
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