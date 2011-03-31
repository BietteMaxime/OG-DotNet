using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles getting values from the engine useful for creating snapshots
    /// 
    /// TODO: this implementation probably shouldn't be client side
    /// TODO: we fetch way more data then I think is neccesary
    /// </summary>
    internal class MarketDataSnapshotHelper : DisposableBase
    {
        internal const string YieldCurveValueReqName = "YieldCurve";
        internal const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        internal const string MarketValueReqName = "Market_Value";

        
        private readonly RemoteEngineContext _remoteEngineContext;

        public MarketDataSnapshotHelper(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
        }

        public ViewComputationResultModel GetAllResults(RemoteView view, DateTimeOffset valuationTime, Dictionary<ValueRequirement, double > overrides)
        {
            var yieldCurveSpecReqs = GetYieldCurveSpecReqs(view, valuationTime);

            var allDataViewDefn = GetTempViewDefinition(view, yieldCurveSpecReqs);
            return RunOneCycle(allDataViewDefn, valuationTime, overrides);
        }
        public ViewComputationResultModel GetAllResults(RemoteView view, DateTimeOffset valuationTime)
        {
            return GetAllResults(view, valuationTime, new Dictionary<ValueRequirement, double>());
        }


        private IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(RemoteView view, DateTimeOffset valuationTime)
        {//TODO this is sloooow
            var tempViewDefinition = GetTempViewDefinition(view);
            var tempResults = RunOneCycle(tempViewDefinition, valuationTime, new Dictionary<ValueRequirement, double>());

            return GetYieldCurveSpecReqs(tempResults);
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
                        foreach (var @override in overrides)
                        {
                            tempView.LiveDataOverrideInjector.AddValue(@override.Key, @override.Value);
                        }
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

        private static IEnumerable<ValueRequirement> GetYieldCurveSpecReqs(ViewComputationResultModel tempResults)
        {
            return tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == YieldCurveValueReqName).Select(r => r.ComputedValue.Specification).Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Properties));
        }

        private static ViewDefinition GetTempViewDefinition(RemoteView view, IEnumerable<ValueRequirement> extraReqs = null)
        {
            extraReqs = extraReqs ?? Enumerable.Empty<ValueRequirement>();

            var tempViewName = string.Format("{0}.{1}.{2}", typeof(MarketDataSnapshotManager).Name, view.Name, Guid.NewGuid());

            return new ViewDefinition(tempViewName, new ResultModelDefinition(ResultOutputMode.All),
                                      view.Definition.PortfolioIdentifier,
                                      view.Definition.User,
                                      view.Definition.DefaultCurrency,
                                      view.Definition.MinDeltaCalcPeriod,
                                      view.Definition.MaxDeltaCalcPeriod,
                                      view.Definition.MinFullCalcPeriod,
                                      view.Definition.MaxFullCalcPeriod,
                                      AddReqs(view.Definition.CalculationConfigurationsByName, extraReqs)
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


        protected override void Dispose(bool disposing)
        {
            //TODO -I'm going to need thsi in order to be less slow
        }
    }
}