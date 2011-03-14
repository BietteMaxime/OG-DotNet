using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    public class MarketDataSnapshotProcessor
    {
        

        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly string _viewName;


        public MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, string viewName)
        {
            _remoteEngineContext = remoteEngineContext;
            _viewName = viewName;
        }


        private RemoteView View
        {
            get { return _remoteEngineContext.ViewProcessor.GetView(_viewName); }
        }
        private ViewDefinition ViewDefinition
        {
            get { return View.Definition; }
        }


        public Dictionary<Pair<string, Currency>, Tuple<YieldCurve, InterpolatedYieldCurveSpecification>> GetYieldCurves(ManageableMarketDataSnapshot snapshot)
        {

            return snapshot.YieldCurves.ToDictionary(kvp => kvp.Key, kvp => GetYieldCurve(snapshot, kvp));
        }

        private Tuple<YieldCurve, InterpolatedYieldCurveSpecification> GetYieldCurve(ManageableMarketDataSnapshot snapshot, KeyValuePair<Pair<string, Currency>, YieldCurveSnapshot> yieldCurveSnapshot)
        {
            var viewDefinition = ViewDefinition;

            var viewDefn = new ViewDefinition(GetViewDefnName(), new ResultModelDefinition(ResultOutputMode.TerminalOutputs),
                                   viewDefinition.PortfolioIdentifier, viewDefinition.User,
                                   viewDefinition.DefaultCurrency, null, null, null, null,
                                   new Dictionary<string, ViewCalculationConfiguration>()
                                       {
                                           {"Default", GetCalcConfig(snapshot, yieldCurveSnapshot, viewDefinition)}
                                       });
            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefn));

                var remoteView = _remoteEngineContext.ViewProcessor.GetView(viewDefn.Name);
                remoteView.Init();
                using (var remoteViewClient = remoteView.CreateClient())
                {
                    foreach (var valueSnapshot in yieldCurveSnapshot.Value.Values)
                    {
                        remoteView.LiveDataOverrideInjector.AddValue(GetOverrideReq(valueSnapshot), valueSnapshot.Value.OverrideValue ?? valueSnapshot.Value.MarketValue  );
                        
                    }

                    var viewComputationResultModel = remoteViewClient.RunOneCycle(yieldCurveSnapshot.Value.ValuationTime);
                    

                    object curve;
                    if (!viewComputationResultModel.TryGetValue("Default", GetYieldCurveReq(yieldCurveSnapshot), out curve))
                    {
                        throw new ArgumentException();
                    }
                    object spec;
                    if (!viewComputationResultModel.TryGetValue("Default", GetYieldCurveSpecReq(yieldCurveSnapshot), out spec))
                    {
                        throw new ArgumentException();
                    }

                    return Tuple.Create((YieldCurve)curve, (InterpolatedYieldCurveSpecification) spec);
                }
            }
        }

        private static ValueRequirement GetOverrideReq(KeyValuePair<Identifier, ValueSnapshot> valueSnapshot)
        {
            return new ValueRequirement(MarketDataSnapshotManager.MarketValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Parse(valueSnapshot.Value.Security.ToString())));
        }

        private ViewCalculationConfiguration GetCalcConfig(ManageableMarketDataSnapshot snapshot, KeyValuePair<Pair<string,Currency>,YieldCurveSnapshot> yieldCurveSnapshot, ViewDefinition viewDefinition)
        {
            return new ViewCalculationConfiguration("Default", 
                new List<ValueRequirement> {GetYieldCurveReq(yieldCurveSnapshot), GetYieldCurveSpecReq(yieldCurveSnapshot)},
                ViewDefinition.CalculationConfigurationsByName.Values.First().PortfolioRequirementsBySecurityType, ViewDefinition.CalculationConfigurationsByName.Values.First().DefaultProperties //TODO can I use DefaultProperties here?
                );
        }

        private static ValueRequirement GetYieldCurveReq(KeyValuePair<Pair<string, Currency>, YieldCurveSnapshot> yieldCurveSnapshot)
        {
            return new ValueRequirement(MarketDataSnapshotManager.YieldCurveValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, yieldCurveSnapshot.Key.Second.Identifier));
        }
        private static ValueRequirement GetYieldCurveSpecReq(KeyValuePair<Pair<string, Currency>, YieldCurveSnapshot> yieldCurveSnapshot)
        {
            return new ValueRequirement(MarketDataSnapshotManager.YieldCurveSpecValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, yieldCurveSnapshot.Key.Second.Identifier));
        }

        private static string GetViewDefnName()
        {
            return typeof(MarketDataSnapshotProcessor).Name + Guid.NewGuid();
        }
    }
}