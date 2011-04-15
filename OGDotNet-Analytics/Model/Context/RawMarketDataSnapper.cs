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
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;
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
            ct.ThrowIfCancellationRequested();

            ct.ThrowIfCancellationRequested();
            InMemoryViewComputationResultModel allResults = GetAllResults(valuationTime, ct);

            ct.ThrowIfCancellationRequested();
            //var requiredLiveData = _view.GetRequiredLiveData();
            throw new NotImplementedException();

            //ct.ThrowIfCancellationRequested();
            //return new ManageableMarketDataSnapshot(_view.Name, GetUnstructuredData(allResults, requiredLiveData), GetYieldCurves(allResults));
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

            return new ManageableYieldCurveSnapshot(values, tempResults.ValuationTime.ToDateTimeOffset());
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
        public InMemoryViewComputationResultModel GetAllResults(DateTimeOffset valuationTime, Dictionary<ValueRequirement, double> overrides, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
            /*
            ct.ThrowIfCancellationRequested();
            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(_view, valuationTime);

            ct.ThrowIfCancellationRequested();
            var allDataViewDefn = GetTempViewDefinition(_view, new ResultModelDefinition(ResultOutputMode.All), yieldCurveSpecReqs);

            ct.ThrowIfCancellationRequested();
            var viewComputationResultModel = RunOneCycle(allDataViewDefn, valuationTime, overrides);

            return viewComputationResultModel;*/
        }

        private InMemoryViewComputationResultModel GetAllResults(DateTimeOffset valuationTime, CancellationToken ct)
        {
            return GetAllResults(valuationTime, new Dictionary<ValueRequirement, double>(), ct);
        }

        private IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(ViewDefinition viewDefinition, DateTimeOffset valuationTime)
        {//TODO this is sloooow
            var tempViewDefinition = GetTempViewDefinition(viewDefinition, new ResultModelDefinition(ResultOutputMode.All));
            var tempResults = RunOneCycle(tempViewDefinition, valuationTime, new Dictionary<ValueRequirement, double>());

            return GetYieldCurveSpecReqs(tempResults);
        }

        public ViewDefinition GetViewOfSnapshot(Dictionary<ValueRequirement, double> overrides)
        {
            var viewDefinition = _remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(_definition.Name);
            var tempViewDefinition = GetTempViewDefinition(_definition, viewDefinition.ResultModelDefinition);
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefinition));
                //var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewDefinition.Name);
                //tempView.Init();
                //ApplyOverrides(tempView, overrides);
                //return tempView;
                throw new NotImplementedException();
            }
        }

        private InMemoryViewComputationResultModel RunOneCycle(ViewDefinition tempViewDefinition, DateTimeOffset valuationTime, Dictionary<ValueRequirement, double> overrides)
        {
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefinition));
                try
                {
                    /*
                    var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewDefinition.Name);

                    tempView.Init();
                    using (var remoteViewClient = tempView.CreateClient())
                    {
                        ApplyOverrides(tempView, overrides);
                        var tempResults = remoteViewClient.RunOneCycle(valuationTime);

                        return tempResults;
                    }
                     * */
                    throw new NotImplementedException();
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewDefinition.Name);
                }
            }
        }

        private static IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(InMemoryViewComputationResultModel tempResults)
        {
            return tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveValueReqName).Select(r => r.ComputedValue.Specification).Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Properties));
        }

        private ViewDefinition GetTempViewDefinition(ViewDefinition viewDefinition, ResultModelDefinition resultModelDefinition, IEnumerable<ValueRequirement> extraReqs = null)
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