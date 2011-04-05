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
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles getting values from the engine useful for creating snapshots
    /// 
    /// TODO: this implementation is evidence for the fact that the Snapshotting shouldn't be client
    /// TODO: we fetch way more data then I think is neccesary
    /// </summary>
    internal class RawMarketDataSnapper : DisposableBase
    {
        internal const string YieldCurveValueReqName = "YieldCurve";
        internal const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        private const string MarketValueReqName = "Market_Value";

        
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly RemoteView _view;

        public RawMarketDataSnapper(RemoteEngineContext remoteEngineContext, RemoteView view)
        {
            _remoteEngineContext = remoteEngineContext;
            _view = view;
        }

        public RemoteEngineContext RemoteEngineContext
        {
            get { return _remoteEngineContext; }
        }

        public RemoteView View
        {
            get { return _view; }
        }

        #region create snapshot
        public ManageableMarketDataSnapshot CreateSnapshotFromView(DateTimeOffset valuationTime, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _view.Init();

            ct.ThrowIfCancellationRequested();
            ViewComputationResultModel allResults = GetAllResults(valuationTime, ct);
            
            ct.ThrowIfCancellationRequested();
            var requiredLiveData = _view.GetRequiredLiveData();
            
            ct.ThrowIfCancellationRequested();
            return new ManageableMarketDataSnapshot(_view.Name, GetUnstructuredData(allResults, requiredLiveData), GetYieldCurves(allResults));
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(ViewComputationResultModel tempResults)
        {

            var resultsByKey = tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveSpecValueReqName)
                .Select(r => (InterpolatedYieldCurveSpecificationWithSecurities)r.ComputedValue.Value)
                .ToLookup(GetYieldCurveKey, s => s);

            var resultByKey = resultsByKey.ToDictionary(g => g.Key, g => g.First());

            return resultByKey.ToDictionary(g => g.Key, g => GetYieldCurveSnapshot(g.Value, tempResults));
        }


        private static YieldCurveKey GetYieldCurveKey(InterpolatedYieldCurveSpecificationWithSecurities spec)
        {
            return new YieldCurveKey(Currency.Create(spec.Currency), spec.Name);
        }



        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(InterpolatedYieldCurveSpecificationWithSecurities spec, ViewComputationResultModel tempResults)
        {
            ManageableUnstructuredMarketDataSnapshot values = GetUnstructuredSnapshot(tempResults, spec);

            return new ManageableYieldCurveSnapshot(values, tempResults.ValuationTime.ToDateTimeOffset());
        }

        private static ManageableUnstructuredMarketDataSnapshot GetUnstructuredSnapshot(ViewComputationResultModel tempResults, InterpolatedYieldCurveSpecificationWithSecurities yieldCurveSpec)
        {
            //TODO do yield curves only take primitive market values?
            IEnumerable<ValueRequirement> reqs = yieldCurveSpec.Strips.Select(s => new ValueRequirement(MarketValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Of(s.SecurityIdentifier))));
            return GetUnstructuredData(tempResults, reqs);
        }


        private static ManageableUnstructuredMarketDataSnapshot GetUnstructuredData(ViewComputationResultModel tempResults, IEnumerable<ValueRequirement> requiredDataSet)
        {
            var dict = GetMatchingData(requiredDataSet, tempResults)
                .ToLookup(r => new MarketDataValueSpecification(GetMarketType(r.Specification.TargetSpecification.Type), r.Specification.TargetSpecification.Uid))
                .ToDictionary(r => r.Key, r => (IDictionary<string, ValueSnapshot>)r.ToDictionary(e => e.Specification.ValueName, e => new ValueSnapshot((double)e.Value)));

            return new ManageableUnstructuredMarketDataSnapshot(dict);
        }

        private static IEnumerable<ComputedValue> GetMatchingData(IEnumerable<ValueRequirement> requiredDataSet, ViewComputationResultModel tempResults)
        {
            foreach (var valueRequirement in requiredDataSet)
            {
                foreach (var config in tempResults.CalculationResultsByConfiguration.Keys)
                {

                    ComputedValue ret;
                    if (tempResults.TryGetComputedValue(config, valueRequirement, out ret))
                    {//TODO what should I do if I can't get a value
                        yield return ret;
                        break;
                    }
                }
                //TODO what should I do if I can't get a value
            }
        }


        private static MarketDataValueType GetMarketType(ComputationTargetType type)
        {
            return type.ConvertTo<MarketDataValueType>();
        }
        #endregion

        #region view defn building
        public ViewComputationResultModel GetAllResults(DateTimeOffset valuationTime, Dictionary<ValueRequirement, double> overrides, CancellationToken ct=default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();
            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(_view, valuationTime);
            
            ct.ThrowIfCancellationRequested();
            var allDataViewDefn = GetTempViewDefinition(_view, new ResultModelDefinition(ResultOutputMode.All), yieldCurveSpecReqs);
            
            ct.ThrowIfCancellationRequested();
            var viewComputationResultModel = RunOneCycle(allDataViewDefn, valuationTime, overrides);
            
            
            return viewComputationResultModel;
        }

        private ViewComputationResultModel GetAllResults(DateTimeOffset valuationTime, CancellationToken ct)
        {
            return GetAllResults(valuationTime, new Dictionary<ValueRequirement, double>(), ct);
        }


        private IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(RemoteView view, DateTimeOffset valuationTime)
        {//TODO this is sloooow
            var tempViewDefinition = GetTempViewDefinition(view, new ResultModelDefinition(ResultOutputMode.All));
            var tempResults = RunOneCycle(tempViewDefinition, valuationTime, new Dictionary<ValueRequirement, double>());

            return GetYieldCurveSpecReqs(tempResults);
        }

        public RemoteView GetViewOfSnapshot(Dictionary<ValueRequirement, double> overrides)
        {
            var tempViewDefinition = GetTempViewDefinition(_view, _view.Definition.ResultModelDefinition);
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefinition));
                var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewDefinition.Name);
                tempView.Init();
                ApplyOverrides(tempView, overrides);
                return tempView;
            }
        }

        private ViewComputationResultModel RunOneCycle(ViewDefinition tempViewDefinition, DateTimeOffset valuationTime, Dictionary<ValueRequirement, double> overrides)
        {
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefinition));
                try
                {
                    var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewDefinition.Name);

                    tempView.Init();
                    using (var remoteViewClient = tempView.CreateClient())
                    {
                        ApplyOverrides(tempView, overrides);
                        var tempResults = remoteViewClient.RunOneCycle(valuationTime);

                        return tempResults;
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewDefinition.Name);
                }
            }
        }

        private static void ApplyOverrides(RemoteView tempView, Dictionary<ValueRequirement, double> overrides)
        {
            foreach (var @override in overrides)
            {
                tempView.LiveDataOverrideInjector.AddValue(@override.Key, @override.Value);
            }
        }

        private static IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(ViewComputationResultModel tempResults)
        {
            return tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveValueReqName).Select(r => r.ComputedValue.Specification).Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Properties));
        }

        private static ViewDefinition GetTempViewDefinition(RemoteView view, ResultModelDefinition resultModelDefinition, IEnumerable<ValueRequirement> extraReqs = null)
        {
            extraReqs = extraReqs ?? Enumerable.Empty<ValueRequirement>();

            var tempViewName = string.Format("{0}.{1}.{2}", typeof(MarketDataSnapshotManager).Name, view.Name, Guid.NewGuid());
            
            var viewDefinition = view.Definition;

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
            return new ViewCalculationConfiguration(calculationConfigurationsByName.Name,
                                                    calculationConfigurationsByName.SpecificRequirements.Concat(
                                                        extraReqs).ToList(),
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