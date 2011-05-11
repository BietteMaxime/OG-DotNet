//-----------------------------------------------------------------------
// <copyright file="RawMarketDataSnapper.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Utils;

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
        private Tuple<ICompiledViewDefinition, IEnumerable<ValueRequirement>> _yieldCurveSpecReqsCache;

        public RawMarketDataSnapper(RemoteEngineContext remoteEngineContext, ViewDefinition definition)
        {
            _remoteEngineContext = remoteEngineContext;
            _definition = definition;
        }

        #region create snapshot
        public ManageableMarketDataSnapshot CreateSnapshotFromView(DateTimeOffset valuationTime, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            ct.ThrowIfCancellationRequested();
            var t = GetAllResults(valuationTime, ct);
            InMemoryViewComputationResultModel allResults = t.Item2;
            var compiledViewDefinition = t.Item1;

            ct.ThrowIfCancellationRequested();
            var requiredLiveData = compiledViewDefinition.LiveDataRequirements;
            
            ct.ThrowIfCancellationRequested();
            return new ManageableMarketDataSnapshot(_definition.Name, GetUnstructuredData(allResults, requiredLiveData.Keys), GetYieldCurves(allResults));
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(InMemoryViewComputationResultModel tempResults)
        {
            var resultsByKey = tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveSpecValueReqName)
                .Select(r => (InterpolatedYieldCurveSpecificationWithSecurities)r.ComputedValue.Value)
                .ToLookup(GetYieldCurveKey, s => s);

            var resultByKey = resultsByKey.ToDictionary(g => g.Key, g => g.First());

            return resultByKey.ToDictionary(g => g.Key, g => GetYieldCurveSnapshot(g.Value, tempResults));
        }

        private static YieldCurveKey GetYieldCurveKey(InterpolatedYieldCurveSpecificationWithSecurities spec)
        {
            return new YieldCurveKey(spec.Currency, spec.Name);
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(InterpolatedYieldCurveSpecificationWithSecurities spec, InMemoryViewComputationResultModel tempResults)
        {
            ManageableUnstructuredMarketDataSnapshot values = GetUnstructuredSnapshot(tempResults, spec);

            return new ManageableYieldCurveSnapshot(values, tempResults.ValuationTime);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetUnstructuredSnapshot(InMemoryViewComputationResultModel tempResults, InterpolatedYieldCurveSpecificationWithSecurities yieldCurveSpec)
        {
            //TODO do yield curves only take primitive market values?
            IEnumerable<ValueRequirement> reqs = yieldCurveSpec.Strips.Select(s => new ValueRequirement(MarketValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Of(s.SecurityIdentifier))));
            return GetUnstructuredData(tempResults, reqs);
        }

        private static ManageableUnstructuredMarketDataSnapshot GetUnstructuredData(InMemoryViewComputationResultModel tempResults, IEnumerable<ValueRequirement> requiredDataSet)
        {
            var dict = GetMatchingData(requiredDataSet, tempResults)
                .ToLookup(r => new MarketDataValueSpecification(GetMarketType(r.Specification.TargetSpecification.Type), r.Specification.TargetSpecification.Uid))
                .ToDictionary(r => r.Key, r => (IDictionary<string, ValueSnapshot>)r.ToDictionary(e => e.Specification.ValueName, e => new ValueSnapshot((double)e.Value)));

            return new ManageableUnstructuredMarketDataSnapshot(dict);
        }

        private static IEnumerable<ComputedValue> GetMatchingData(IEnumerable<ValueRequirement> requiredDataSet, InMemoryViewComputationResultModel tempResults)
        {
            foreach (var valueRequirement in requiredDataSet)
            {
                foreach (var config in tempResults.CalculationResultsByConfiguration.Keys)
                {
                    ComputedValue ret;
                    if (tempResults.TryGetComputedValue(config, valueRequirement, out ret))
                    { // TODO what should I do if I can't get a value
                        yield return ret;
                        break;
                    }
                }
                //TODO what should I do if I can't get a value
            }
        }

        private static MarketDataValueType GetMarketType(ComputationTargetType type)
        {
            return EnumUtils<ComputationTargetType, MarketDataValueType>.ConvertTo(type);
        }
        #endregion

        #region view defn building

        public Tuple<ICompiledViewDefinition, InMemoryViewComputationResultModel> GetAllResults(DateTimeOffset valuationTime, UniqueIdentifier snapshotIdentifier, CancellationToken ct = default(CancellationToken))
        {
            ViewDefinition allDataViewDefn = GetAllDataViewDefn(ct, valuationTime);

            ct.ThrowIfCancellationRequested();
            return RunOneCycle(allDataViewDefn, snapshotIdentifier);
        }

        private ViewDefinition GetAllDataViewDefn(CancellationToken ct, DateTimeOffset valuationTime)
        {
            ct.ThrowIfCancellationRequested();
            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(_definition, valuationTime);

            ct.ThrowIfCancellationRequested();
            return GetTempViewDefinition(_definition, new ResultModelDefinition(ResultOutputMode.All), yieldCurveSpecReqs);
        }

        private Tuple<ICompiledViewDefinition, InMemoryViewComputationResultModel> GetAllResults(DateTimeOffset valuationTime, CancellationToken ct)
        {
            ViewDefinition allDataViewDefn = GetAllDataViewDefn(ct, valuationTime);

            ct.ThrowIfCancellationRequested();
            return RunOneCycle(allDataViewDefn, valuationTime);
        }

        private IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(ViewDefinition viewDefinition, DateTimeOffset valuationTime)
        {
            var cached = _yieldCurveSpecReqsCache;
            if (cached != null && cached.Item1.IsValidFor(valuationTime))
            {
                return cached.Item2;
            }

            //TODO this is sloooow
            var tempViewDefinition = GetTempViewDefinition(viewDefinition, new ResultModelDefinition(ResultOutputMode.All));
            var tempResults = RunOneCycle(tempViewDefinition, valuationTime);

            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(tempResults.Item2);
            _yieldCurveSpecReqsCache = Tuple.Create(tempResults.Item1, yieldCurveSpecReqs);
            return yieldCurveSpecReqs;
        }

        private Tuple<ICompiledViewDefinition, InMemoryViewComputationResultModel> RunOneCycle(ViewDefinition tempViewDefinition, UniqueIdentifier snapshotIdentifier)
        {
            var options = ExecutionOptions.Snapshot(snapshotIdentifier);
            return RunOneCycle(tempViewDefinition, options);
        }

        private Tuple<ICompiledViewDefinition, InMemoryViewComputationResultModel> RunOneCycle(ViewDefinition tempViewDefinition, DateTimeOffset valuationTime)
        {
            var options = ExecutionOptions.Batch(ArbitraryViewCycleExecutionSequence.Of(valuationTime));
            var results = RunOneCycle(tempViewDefinition, options);
            return results;
        }

        private Tuple<ICompiledViewDefinition, InMemoryViewComputationResultModel> RunOneCycle(ViewDefinition tempViewDefinition, IViewExecutionOptions options)
        {
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefinition));
                try
                {
                    using (var completedEvent = new ManualResetEvent(false))
                    using (var remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient())
                    {
                        var cycles = new ConcurrentQueue<CycleCompletedArgs>();
                        var compiles = new ConcurrentQueue<ViewDefinitionCompiledArgs>();
                        var errors = new ConcurrentQueue<object>();

                        var listener = new EventViewResultListener();
                        listener.ViewDefinitionCompiled += (sender, e) => compiles.Enqueue(e);
                        listener.CycleCompleted += (sender, e) => cycles.Enqueue(e);

                        listener.ViewDefinitionCompilationFailed += (sender, e) =>
                                                                        {
                                                                            completedEvent.Set();
                                                                            errors.Enqueue(e);
                                                                        };
                        listener.CycleExecutionFailed += (sender, e) => 
                                                                        {
                                                                            completedEvent.Set();
                                                                            errors.Enqueue(e);
                                                                        };
                        listener.ProcessCompleted += (sender, e) => completedEvent.Set();

                        remoteViewClient.SetResultListener(listener);

                        remoteViewClient.AttachToViewProcess(tempViewDefinition.Name, options);

                        completedEvent.WaitOne();
                        if (errors.Any())
                        {
                            var openGammaException = new OpenGammaException("Error occured when executing view");
                            openGammaException.Data["ExecErrors"] = errors.ToList();
                            throw openGammaException;
                        }
                        return Tuple.Create(compiles.Single().CompiledViewDefinition, cycles.Single().FullResult);
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewDefinition.Name);
                }
            }
        }

        private static IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(InMemoryViewComputationResultModel tempResults)
        {
            return tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveValueReqName).Select(r => r.ComputedValue.Specification).Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Properties)).ToList();
        }

        private static ViewDefinition GetTempViewDefinition(ViewDefinition viewDefinition, ResultModelDefinition resultModelDefinition, IEnumerable<ValueRequirement> extraReqs = null)
        {
            extraReqs = extraReqs ?? Enumerable.Empty<ValueRequirement>();

            var tempViewName = string.Format("{0}.{1}.{2}", typeof(MarketDataSnapshotManager).Name, viewDefinition.Name, Guid.NewGuid());

            return new ViewDefinition(tempViewName, resultModelDefinition,
                                      viewDefinition.PortfolioIdentifier,
                                      viewDefinition.User,
                                      viewDefinition.DefaultCurrency,
                                      viewDefinition.MinDeltaCalcPeriod,
                                      viewDefinition.MaxDeltaCalcPeriod,
                                      viewDefinition.MinFullCalcPeriod,
                                      viewDefinition.MaxFullCalcPeriod,
                                      AddReqs(viewDefinition.CalculationConfigurationsByName, extraReqs)
                );
        }

        private static Dictionary<string, ViewCalculationConfiguration> AddReqs(Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName, IEnumerable<ValueRequirement> extraReqs)
        {
            return calculationConfigurationsByName.ToDictionary(kvp => kvp.Key,
                                                                kvp => AddReqs(kvp.Value, extraReqs)
                );
        }

        private static ViewCalculationConfiguration AddReqs(ViewCalculationConfiguration calculationConfigurationsByName, IEnumerable<ValueRequirement> extraReqs)
        {
            var specificRequirements = calculationConfigurationsByName.SpecificRequirements.Concat(extraReqs).ToList();
            return new ViewCalculationConfiguration(calculationConfigurationsByName.Name,
                                                    specificRequirements,
                                                    calculationConfigurationsByName.PortfolioRequirementsBySecurityType,
                                                    calculationConfigurationsByName.DefaultProperties);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //TODO -I'm going to need this in order to be less slow
        }
    }
}