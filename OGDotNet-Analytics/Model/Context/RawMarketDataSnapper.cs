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
    /// <item>TODO: we fetch way more data then I think is neccesary</item>
    /// </list>
    /// </summary>
    internal class RawMarketDataSnapper : DisposableBase
    {
        internal const string YieldCurveValueReqName = "YieldCurve";
        internal const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        private const string MarketValueReqName = "Market_Value";

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
            
            return WithSingleCycle(delegate(InMemoryViewComputationResultModel results, IViewCycle viewCycle)
                    {
                        var globalValues = GetGlobalValues(results);
                        var yieldCurves = GetYieldCurves(results, viewCycle).ToDictionary(yieldCurve => yieldCurve.Key, yieldCurve => GetYieldCurveSnapshot(yieldCurve.Value.Item2, globalValues, results.ValuationTime));

                        return new ManageableMarketDataSnapshot(_definition.Name, globalValues, yieldCurves);
                    }, ExecutionOptions.GetSingleCycle(valuationTime), ct);
        }

        private static YieldCurveKey GetYieldCurveKey(ValueSpecification y)
        {
            return new YieldCurveKey(Currency.Create(y.TargetSpecification.Uid), y.Properties["Curve"].Single());
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(InterpolatedYieldCurveSpecificationWithSecurities spec, ManageableUnstructuredMarketDataSnapshot tempResults, DateTimeOffset valuationTime)
        {
            var specifications = spec.Strips.Select(s => new MarketDataValueSpecification(MarketDataValueType.Primitive,UniqueIdentifier.Of(s.SecurityIdentifier)));
            var dict = specifications.ToDictionary<MarketDataValueSpecification, MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(s => s, 
                s => new Dictionary<string, ValueSnapshot> {{MarketValueReqName, tempResults.Values[s][MarketValueReqName]}});

            var values = new ManageableUnstructuredMarketDataSnapshot(dict);
            return new ManageableYieldCurveSnapshot(values, valuationTime);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetGlobalValues(InMemoryViewComputationResultModel tempResults)
        {
            var data = tempResults.LiveData;
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
            return WithSingleCycle(GetYieldCurves, ExecutionOptions.Snapshot(snapshotIdentifier, DateTimeOffset.Now), ct);
        }

        private static Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(InMemoryViewComputationResultModel results, IViewCycle viewCycle)
        {
            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(results, YieldCurveSpecValueReqName, YieldCurveValueReqName);
            var yieldCurves = new Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>>();

            foreach (var yieldCurveSpecReq in yieldCurveSpecReqs)
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
                    var value = (YieldCurve) result.Where(r => r.First.ValueName == YieldCurveValueReqName).Single().Second;
                    var spec = (InterpolatedYieldCurveSpecificationWithSecurities) result.Where(r => r.First.ValueName == YieldCurveSpecValueReqName).Single().Second;
                    yieldCurves.Add(result.Key, Tuple.Create(value, spec));
                }
            }

            return yieldCurves;
        }

        private T WithSingleCycle<T>(Func<InMemoryViewComputationResultModel, IViewCycle, T> func, IViewExecutionOptions executionOptions, CancellationToken ct)
        {
            CheckDisposed();

            using (var completed = new ManualResetEventSlim(false))
            using (var remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient())
            {
                InMemoryViewComputationResultModel results = null;
                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ProcessCompleted += delegate { completed.Set(); };
                eventViewResultListener.CycleCompleted +=
                    delegate(object sender, CycleCompletedArgs e) { results = e.FullResult; };
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

        private static Dictionary<string, IEnumerable<ValueSpecification>> GetYieldCurveSpecReqs(InMemoryViewComputationResultModel tempResults, params string[] yieldCurveValues)
        {
            //TODO: LAPANA-50 should be done from the dep graph
            return tempResults.CalculationResultsByConfiguration.ToDictionary(config=>config.Key, config =>
            config.Value.AllResults.Where(r => r.Specification.ValueName == YieldCurveValueReqName).Select(r => r.Specification)
            .SelectMany(r => yieldCurveValues.Select(name => new ValueSpecification(name, r.TargetSpecification, r.Properties))))
            ;
        }

        protected override void Dispose(bool disposing)
        {
            //TODO -I'm going to need this in order to be less slow
        }
    }
}