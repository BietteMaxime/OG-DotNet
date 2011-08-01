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
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Utils;
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
        static readonly StructuredSnapper<YieldCurveKey, SnapshotDataBundle, ManageableYieldCurveSnapshot> YieldCurveSnapper = new StructuredSnapper<YieldCurveKey, SnapshotDataBundle, ManageableYieldCurveSnapshot>(
            ValueRequirementNames.YieldCurveMarketData, GetYieldCurveKey, (remoteEngineContext, results, k, o) => GetYieldCurveSnapshot(o, results.ValuationTime)
            );

        static readonly StructuredSnapper<VolatilitySurfaceKey, VolatilitySurfaceData, ManageableVolatilitySurfaceSnapshot> SurfaceSnapper = new StructuredSnapper<VolatilitySurfaceKey, VolatilitySurfaceData, ManageableVolatilitySurfaceSnapshot>(
            ValueRequirementNames.VolatilitySurfaceData, GetVolSurfaceKey, (remoteEngineContext, results, k, o) => GetVolSurfaceSnapshot(o)
            );

        static readonly StructuredSnapper<VolatilityCubeKey, VolatilityCubeData, ManageableVolatilityCubeSnapshot> CubeSnapper = new StructuredSnapper<VolatilityCubeKey, VolatilityCubeData, ManageableVolatilityCubeSnapshot>(
            ValueRequirementNames.VolatilityCubeMarketData, GetVolCubeKey, (remoteEngineContext, results, k, o) => GetVolCubeSnapshot(o, remoteEngineContext.VolatilityCubeDefinitionSource.GetDefinition(k.Currency, k.Name))
            );

        static readonly StructuredSnapper[] Snappers = new StructuredSnapper[] { CubeSnapper, SurfaceSnapper, YieldCurveSnapper };

        #region create snapshot

        public static ManageableMarketDataSnapshot CreateSnapshotFromCycle(IViewComputationResultModel results, IDictionary<string, IDependencyGraph> graphs, IViewCycle viewCycle, string basisViewName, RemoteEngineContext remoteEngineContext)
        {
            var globalValues = GetGlobalValues(results, graphs);
            
            var yieldCurves = YieldCurveSnapper.GetValues(results, graphs, viewCycle, remoteEngineContext);
            var volCubeDefinitions = CubeSnapper.GetValues(results, graphs, viewCycle, remoteEngineContext);
            var volSurfaceDefinitions = SurfaceSnapper.GetValues(results, graphs, viewCycle, remoteEngineContext);

            return new ManageableMarketDataSnapshot(basisViewName, globalValues, yieldCurves, volCubeDefinitions, volSurfaceDefinitions);
        }

        private static YieldCurveKey GetYieldCurveKey(ValueSpecification y)
        {
            var computationTargetSpecification = y.TargetSpecification;

            var valueProperties = y.Properties;
            return GetYieldCurveKey(computationTargetSpecification, valueProperties);
        }

        private static YieldCurveKey GetYieldCurveKey(ComputationTargetSpecification computationTargetSpecification, ValueProperties valueProperties)
        {
            return new YieldCurveKey(Currency.Create(computationTargetSpecification.Uid), valueProperties["Curve"].Single());
        }

        private static VolatilityCubeKey GetVolCubeKey(ValueSpecification y)
        {
            return new VolatilityCubeKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Cube"].Single());
        }

        private static VolatilitySurfaceKey GetVolSurfaceKey(ValueSpecification y)
        {
            return new VolatilitySurfaceKey(y.TargetSpecification.Uid, y.Properties["Surface"].Single(), y.Properties["InstrumentType"].Single());
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
                s => (IDictionary<string, ValueSnapshot>)new Dictionary<string, ValueSnapshot> { { ValueRequirementNames.MarketValue, new ValueSnapshot(s.Value) } }
                );
            return new ManageableUnstructuredMarketDataSnapshot(data);
        }

        private static ManageableVolatilitySurfaceSnapshot GetVolSurfaceSnapshot(VolatilitySurfaceData volatilitySurfaceData)
        {
            return GenericUtils.Call<ManageableVolatilitySurfaceSnapshot>(typeof(RawMarketDataSnapper), "GetVolSurfaceSnapshot", typeof(VolatilitySurfaceData<,>), volatilitySurfaceData);
        }
        public static ManageableVolatilitySurfaceSnapshot GetVolSurfaceSnapshot<TX, TY>(VolatilitySurfaceData<TX, TY> volatilitySurfaceData)
        {
            IDictionary<Pair<object, object>, ValueSnapshot> dict = new Dictionary<Pair<object, object>, ValueSnapshot>();
            foreach (var x in volatilitySurfaceData.Xs)
            {
                foreach (var y in volatilitySurfaceData.Ys)
                {
                    var key = new Pair<object, object>(x, y);
                    double value;
                    if (volatilitySurfaceData.TryGet(x, y, out value))
                    {
                        dict.Add(key, new ValueSnapshot(value));
                    }
                    else
                    {
                        dict.Add(key, new ValueSnapshot(null));
                    }
                }
            }
            
            return new ManageableVolatilitySurfaceSnapshot(dict);
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
            return Snappers.Any(s => IsStructuredMarketDataNode(node, s));
        }

        private static bool IsStructuredMarketDataNode(DependencyNode node, StructuredSnapper snapper)
        {
            Func<ValueSpecification, bool> snapperReqPredicate = v => v.ValueName == snapper.RequirementName;
            var isStructuredNode = node.OutputValues.Any(snapperReqPredicate);
            if (isStructuredNode && !node.OutputValues.All(snapperReqPredicate))
            {
                throw new ArgumentException(string.Format("Unsure how to handle node {0}", node));
            }
            return isStructuredNode;
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
            var matchingSpecifications = GetYieldCurveSpecifications(graphs, ValueRequirementNames.YieldCurve, ValueRequirementNames.YieldCurveSpec);
            var included = matchingSpecifications.SelectMany(s => s.Value);
            var interpolated = included.Where(s => s.ValueName == ValueRequirementNames.YieldCurve).Select(
                sp => new ValueSpecification(ValueRequirementNames.YieldCurveInterpolated, sp.TargetSpecification, GetCurveProperties(sp)));
            return included.Concat(interpolated);
        }

        private static Dictionary<string, IEnumerable<ValueSpecification>> GetYieldCurveSpecifications(IDictionary<string, IDependencyGraph> graphs, params string[] specNames)
        {
            var ret = new Dictionary<string, IEnumerable<ValueSpecification>>();

            foreach (var kvp in graphs)
            {
                var config = kvp.Key;
                var graph = kvp.Value;

                var specs = graph.DependencyNodes.SelectMany(n => n.OutputValues)
                    //This is a hack: the spec value will be pruned from the dep graph but will be in the computation cache,
                    //  and we know that it has exactly the some properties as the Curve value
                    .SelectMany(v => v.ValueName == ValueRequirementNames.YieldCurve ? new[] { v, new ValueSpecification(ValueRequirementNames.YieldCurveSpec, v.TargetSpecification, v.Properties) } : new[] { v })

                    .Where(v => specNames.Contains(v.ValueName));
                ret.Add(config, specs.ToList());
            }

            return ret;
        }

        private static ValueProperties GetCurveProperties(ValueSpecification sp)
        {
            return ValueProperties.Create(
                new Dictionary<string, ISet<string>> { { "Curve", new HashSet<string>(sp.Properties["Curve"]) } },
                new HashSet<string>());
        }

        public static Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> EvaluateYieldCurves(IViewComputationResultModel results, ViewDefinition viewDefinition)
        {
            var ycResults = results.AllResults
                .Where(r => new[] { ValueRequirementNames.YieldCurve, ValueRequirementNames.YieldCurveSpec, ValueRequirementNames.YieldCurveInterpolated }.Contains(r.ComputedValue.Specification.ValueName));
            var lookup = ycResults
                .ToLookup(r => GetYieldCurveKey(r.ComputedValue.Specification));

            var requested = viewDefinition.CalculationConfigurationsByName.Single().Value.SpecificRequirements.ToLookup(
                r => GetYieldCurveKey(r.TargetSpecification, r.Constraints));
            var ret = lookup
                .ToDictionary(g => g.Key,
                              g => GetEvaluatedCurve(g.ToDictionaryDiscardingDuplicates(e => e.ComputedValue.Specification.ValueName, e => e.ComputedValue.Value)));
            var got = new HashSet<YieldCurveKey>(ret.Keys);
            foreach (var missing in requested.Select(g => g.Key).Except(got))
            {
                ret.Add(missing, default(Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>));
            }
            return ret;
        }

        private static Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve> GetEvaluatedCurve(Dictionary<string, object> values)
        {
            return Tuple.Create((YieldCurve)values[ValueRequirementNames.YieldCurve],
                (InterpolatedYieldCurveSpecificationWithSecurities)values[ValueRequirementNames.YieldCurveSpec], (NodalDoublesCurve)values[ValueRequirementNames.YieldCurveInterpolated]);
        }

        public static Dictionary<string, IDependencyGraph> GetGraphs(ICompiledViewDefinitionWithGraphs compiledViewDefinitionWithGraphs)
        {
            return compiledViewDefinitionWithGraphs.CompiledCalculationConfigurations.Keys
                .ToDictionary(k => k, k => compiledViewDefinitionWithGraphs.GetDependencyGraphExplorer(k).GetWholeGraph());
        }
    }
}