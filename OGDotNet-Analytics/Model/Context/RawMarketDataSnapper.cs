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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Math.Curve;
using OGDotNet.Utils;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

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

        public static IEnumerable<ValueSpecification> GetYieldCurveSpecs(IDictionary<string, IDependencyGraph> graphs)
        {
            var matchingSpecifications = GetYieldCurveSpecifications(graphs, ValueRequirementNames.YieldCurve, ValueRequirementNames.YieldCurveSpec);
            var included = matchingSpecifications.SelectMany(s => s.Value);
            var interpolated = included.Where(s => s.ValueName == ValueRequirementNames.YieldCurve).Select(
                sp => new ValueSpecification(ValueRequirementNames.YieldCurveInterpolated, sp.TargetSpecification, GetCurveProperties(sp)));
            return included.Concat(interpolated);
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

        private static Dictionary<string, IEnumerable<ValueSpecification>> GetYieldCurveSpecifications(IDictionary<string, IDependencyGraph> graphs, params string[] specNames)
        {
            var ret = new Dictionary<string, IEnumerable<ValueSpecification>>();

            foreach (var kvp in graphs)
            {
                var config = kvp.Key;
                var graph = kvp.Value;

                var specs = graph.DependencyNodes.SelectMany(n => n.OutputValues)
                    //This is a hack: the spec value will be pruned from the dep graph but will be in the computation cache,
                    //  and we know how its properties relate to the sCurve value
                    .SelectMany(v => v.ValueName == ValueRequirementNames.YieldCurve ? new[] { v, new ValueSpecification(ValueRequirementNames.YieldCurveSpec, v.TargetSpecification, GetSpecProperties(v.Properties)) } : new[] { v })

                    .Where(v => specNames.Contains(v.ValueName));
                ret.Add(config, specs.ToList());
            }

            return ret;
        }

        private static ValueProperties GetSpecProperties(ValueProperties properties)
        {
            return properties.WithoutAny(ValueRequirementNames.CurveCalculationMethod);
        }

        private static ValueProperties GetCurveProperties(ValueSpecification sp)
        {
            return ValueProperties.Create(
                new Dictionary<string, ISet<string>> { { "Curve", new HashSet<string>(sp.Properties["Curve"]) } },
                new HashSet<string>());
        }

        private static Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve> GetEvaluatedCurve(Dictionary<string, object> values)
        {
            return Tuple.Create((YieldCurve)values[ValueRequirementNames.YieldCurve],
                (InterpolatedYieldCurveSpecificationWithSecurities)values[ValueRequirementNames.YieldCurveSpec], (NodalDoublesCurve)values[ValueRequirementNames.YieldCurveInterpolated]);
        }
    }
}