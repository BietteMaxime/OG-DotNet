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
using System.Threading;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
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
    internal class RawMarketDataSnapper : DisposableBase
    {
        private const string YieldCurveValueReqName = "YieldCurve";
        private const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        private const string MarketValueReqName = "Market_Value";
        private const string YieldCurveMarketDataReqName = "YieldCurveMarketData";

        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly ViewDefinition _definition;

        public RawMarketDataSnapper(RemoteEngineContext remoteEngineContext, ViewDefinition definition)
        {
            _remoteEngineContext = remoteEngineContext;
            _definition = definition;
        }

        #region create snapshot
        public ManageableMarketDataSnapshot CreateSnapshotFromView(DateTimeOffset valuationTime, CancellationToken ct)
        {
            CheckDisposed();

            return WithSingleCycle(delegate(ViewComputationResultModel results, IViewCycle viewCycle)
                    {
                        var globalValues = GetGlobalValues(results);
                        var yieldCurves = GetYieldCurveValues(results, viewCycle, YieldCurveMarketDataReqName).ToDictionary(yieldCurve => yieldCurve.Key, yieldCurve => GetYieldCurveSnapshot((SnapshotDataBundle) yieldCurve.Value[YieldCurveMarketDataReqName], globalValues, results.ValuationTime));

                        return new ManageableMarketDataSnapshot(_definition.Name, globalValues, yieldCurves);
                    }, ExecutionOptions.GetSingleCycle(valuationTime), ct);
        }

        private static YieldCurveKey GetYieldCurveKey(ValueSpecification y)
        {
            return new YieldCurveKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Curve"].Single());
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(SnapshotDataBundle bundle, ManageableUnstructuredMarketDataSnapshot tempResults, DateTimeOffset valuationTime)
        {
            var specifications = bundle.DataPoints.Select(s => new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of(s.Key)));
            var dict = specifications.ToDictionary(s => s,
                s => (IDictionary<string, ValueSnapshot>)new Dictionary<string, ValueSnapshot> { { MarketValueReqName, new ValueSnapshot(tempResults.Values[s][MarketValueReqName].MarketValue) } });

            var values = new ManageableUnstructuredMarketDataSnapshot(dict);
            return new ManageableYieldCurveSnapshot(values, valuationTime);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetGlobalValues(ViewComputationResultModel tempResults)
        {
            var data = tempResults.AllLiveData;
            var dataByTarget = data.ToLookup(r => new MarketDataValueSpecification(GetMarketType(r.Specification.TargetSpecification.Type), r.Specification.TargetSpecification.Uid));
            var dict = dataByTarget.ToDictionary(g => g.Key, GroupByValueName);

            return new ManageableUnstructuredMarketDataSnapshot(dict);
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

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(UniqueIdentifier snapshotIdentifier, CancellationToken ct)
        {
            return WithSingleCycle(EvaluateYieldCurves, ExecutionOptions.Snapshot(snapshotIdentifier), ct);
        }

        private static Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> EvaluateYieldCurves(ViewComputationResultModel r, IViewCycle c)
        {
            return GetYieldCurveValues(r, c, YieldCurveValueReqName, YieldCurveSpecValueReqName)
                .ToDictionary(k => k.Key, k => GetEvaluatedCurve(k.Value));
        }

        private static Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetEvaluatedCurve(Dictionary<string, object> values)
        {
            return Tuple.Create((YieldCurve)values[YieldCurveValueReqName],
                (InterpolatedYieldCurveSpecificationWithSecurities) values[YieldCurveSpecValueReqName]);
        }

        private static Dictionary<YieldCurveKey, Dictionary<string, object>> GetYieldCurveValues(ViewComputationResultModel results, IViewCycle viewCycle, params string[] valueNames)
        {
            var values = GetMatchingSpecifications(results, viewCycle, valueNames);
            var yieldCurves = new Dictionary<YieldCurveKey, Dictionary<string, object>>();

            foreach (var yieldCurveSpecReq in values)
            {
                var requiredSpecs = yieldCurveSpecReq.Value.Where(r => !yieldCurves.ContainsKey(GetYieldCurveKey(r)));
                if (!requiredSpecs.Any())
                {
                    continue;
                }
                var computationCacheResponse = viewCycle.QueryComputationCaches(new ComputationCacheQuery(yieldCurveSpecReq.Key, requiredSpecs));

                if (computationCacheResponse.Results.Count != requiredSpecs.Count())
                {
                    throw new ArgumentException("Failed to get all results");
                }

                var yieldCurveInfo = computationCacheResponse.Results.ToLookup(r => GetYieldCurveKey(r.First));
                foreach (var result in yieldCurveInfo)
                {
                    yieldCurves.Add(result.Key, result.ToDictionary(r=>r.First.ValueName, r=>r.Second));
                }
            }

            return yieldCurves;
        }

        private T WithSingleCycle<T>(Func<ViewComputationResultModel, IViewCycle, T> func, IViewExecutionOptions executionOptions, CancellationToken ct)
        {
            CheckDisposed();

            using (var completed = new ManualResetEventSlim(false))
            using (var remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient())
            {
                ViewComputationResultModel results = null;
                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ProcessCompleted += delegate { completed.Set(); };
                eventViewResultListener.CycleCompleted +=
                    delegate(object sender, CycleCompletedArgs e)
                        {
                            results = e.FullResult;
                            completed.Set();
                        };
                remoteViewClient.SetResultListener(eventViewResultListener);

                remoteViewClient.SetViewCycleAccessSupported(true);
                remoteViewClient.AttachToViewProcess(_definition.Name, executionOptions);

                completed.Wait(ct);
                if (results == null)
                {
                    throw new OpenGammaException("Failed to get results");
                }

                using (var engineResourceReference = remoteViewClient.CreateLatestCycleReference())
                {
                    var viewCycle = engineResourceReference.Value;
                    return func(results, viewCycle);
                }
            }
        }

        private static Dictionary<string, IEnumerable<ValueSpecification>> GetMatchingSpecifications(ViewComputationResultModel tempResults,  IViewCycle viewCycle, params string[] specNames)
        {
            var ret = new Dictionary<string, IEnumerable<ValueSpecification>>();
            
            foreach (string config in tempResults.CalculationResultsByConfiguration.Keys)
            {
                var dependencyGraphExplorer = viewCycle.GetCompiledViewDefinition().GetDependencyGraphExplorer(config);
                var graph = dependencyGraphExplorer.GetWholeGraph();
                var specs = graph.DependencyNodes.SelectMany(n => n.OutputValues)
                    //This is a hack: the spec value will be pruned from the dep graph but will be in the computation cache,
                    //  and we know that it has exactly the some properties as the Curve value
                    .SelectMany(v => v.ValueName == YieldCurveValueReqName ?  new[] {v, new ValueSpecification(YieldCurveSpecValueReqName, v.TargetSpecification, v.Properties)}: new[] {v})

                    .Where(v => specNames.Contains(v.ValueName));
                ret.Add(config, specs.ToList());
            }

            return ret;
        }

        protected override void Dispose(bool disposing)
        {
            //TODO -I'm going to need this in order to be less slow
        }
    }
}