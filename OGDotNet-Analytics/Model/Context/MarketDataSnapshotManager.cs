using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles creating and mutating snapshots based on Views and live data
    /// 
    /// TODO: this implementation probably shouldn't be client side
    /// </summary>
    public class MarketDataSnapshotManager
    {
        private readonly RemoteEngineContext _remoteEngineContext;

        public MarketDataSnapshotManager(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
        }

        public ManageableMarketDataSnapshot CreateFromView(string viewName)
        {
            return CreateFromView(viewName, DateTimeOffset.Now);
        }

        private ManageableMarketDataSnapshot CreateFromView(string viewName, DateTimeOffset valuationTime)
        {
            return CreateFromView(_remoteEngineContext.ViewProcessor.GetView(viewName), valuationTime);
        }

        private ManageableMarketDataSnapshot CreateFromView(RemoteView view, DateTimeOffset valuationTime)
        {
            view.Init();
            var requiredLiveData = view.GetRequiredLiveData();

            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                var tempViewName = typeof(MarketDataSnapshotManager).FullName + Guid.NewGuid();

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(GetView(tempViewName, requiredLiveData)));

                var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewName);
                try
                {
                    tempView.Init();
                    using (var remoteViewClient = tempView.CreateClient())
                    {
                        var tempResults = remoteViewClient.RunOneCycle(valuationTime);
                        return new ManageableMarketDataSnapshot { Values = requiredLiveData.ToDictionary(r => r.TargetSpecification.Uid, r => new ValueSnapshot { Security = r.TargetSpecification.Uid, MarketValue = GetValue(tempResults, r) }) };
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewName);                
                }
            }
        }

        private static double GetValue(ViewComputationResultModel tempResults, ValueRequirement valueRequirement)
        {
            object ret;
            if (! tempResults.TryGetValue("Default", valueRequirement, out ret))
            {
                throw new ArgumentException();
            }
            return (double) ret;
        }

        private static ViewDefinition GetView(string name, IEnumerable<ValueRequirement> requiredLiveData)
        {
            return new ViewDefinition(name, new ResultModelDefinition(ResultOutputMode.TerminalOutputs), calculationConfigurationsByName:new Dictionary<string, ViewCalculationConfiguration>
                                                                                                                                                                   {
                                                                                                                                                                       {"Default", new ViewCalculationConfiguration("Default", requiredLiveData.ToList(), new Dictionary<string, ValueProperties>())}
                                                                                                                                                                   });
        }

        public ManageableMarketDataSnapshot UpdateFromView(ManageableMarketDataSnapshot basis, string viewName)
        {
            //TODO handle portfolio/view changing
            var newSnapshot = CreateFromView(viewName);

            return new ManageableMarketDataSnapshot { Values = newSnapshot.Values.ToDictionary(v=>v.Key, v=> new ValueSnapshot{Security = v.Key, MarketValue = v.Value.MarketValue, OverrideValue = basis.Values[v.Key].OverrideValue}) };
        }
    }
}
