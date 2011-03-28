using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles creating and mutating snapshots based on Views and live data
    /// 
    /// TODO: this implementation probably shouldn't be client side
    /// </summary>
    public class MarketDataSnapshotManager : DisposableBase
    {
        internal const string YieldCurveValueReqName = "YieldCurve";
        internal const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        internal const string MarketValueReqName = "Market_Value";

        private static readonly string MarketValuesConfigName = "MarketValues"+Guid.NewGuid();

        private readonly RemoteEngineContext _remoteEngineContext;

        public MarketDataSnapshotManager(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
        }


        public MarketDataSnapshotProcessor GetProcessor(string viewName)
        {
            return new MarketDataSnapshotProcessor(_remoteEngineContext, viewName);    
        }
        public ManageableMarketDataSnapshot CreateFromView(string viewName)
        {
            return CreateFromView(viewName, DateTimeOffset.Now);
        }

        private ManageableMarketDataSnapshot CreateFromView(string viewName, DateTimeOffset valuationTime)
        {
            return CreateFromView(_remoteEngineContext.ViewProcessor.GetView(viewName), valuationTime);
        }

        /// <summary>
        /// TODO Filtering
        /// </summary>
        public void UpdateFromView(ManageableMarketDataSnapshot basis, string viewName)
        {
            
            var newSnapshot = CreateFromView(viewName);

            basis.UpdateFrom(newSnapshot);
        }


        private ManageableMarketDataSnapshot CreateFromView(RemoteView view, DateTimeOffset valuationTime)
        {
            view.Init();

            ViewDefinition tempViewDefn;
            IEnumerable<ValueRequirement> requiredLiveData = GetTempView(view, out tempViewDefn);

            var requiredDataSet = new HashSet<Tuple<ComputationTargetSpecification,string>>(requiredLiveData.Select(vr => Tuple.Create(vr.TargetSpecification, vr.ValueName)));
            Func<ComputationTargetSpecification, string, bool> isRequiredData = (cts, name) => requiredDataSet.Contains(Tuple.Create(cts,name));

            using (var remoteClient = _remoteEngineContext.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(tempViewDefn));
                try
                {
                    var tempView = _remoteEngineContext.ViewProcessor.GetView(tempViewDefn.Name);

                    tempView.Init();
                    using (var remoteViewClient = tempView.CreateClient())
                    {
                        var tempResults = remoteViewClient.RunOneCycle(valuationTime);
                        return new ManageableMarketDataSnapshot(view.Name, GetUnstructuredSnapshot(tempResults, isRequiredData), GetYieldCurves(tempResults));
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewDefn.Name);                
                }
            }
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(ViewComputationResultModel tempResults)
        {
            //TODO less insanity
            var specEntries = tempResults.AllResults.Where(r =>r.ComputedValue.Specification.ValueName==YieldCurveSpecValueReqName);
            var dictionary = specEntries.GroupBy(vre => vre.CalculationConfiguration).ToDictionary(g => g.Key,
                                                                                                         g =>
                                                                                                         g.GroupBy(
                                                                                                             gg =>
                                                                                                             gg.ComputedValue.
                                                                                                                 Specification.
                                                                                                                 TargetSpecification.Uid));
            var curves = dictionary.SelectMany(kvp => kvp.Value.SelectMany(g => g.Select(gg=> Tuple.Create(kvp.Key, g.Key, (InterpolatedYieldCurveSpecification) gg.ComputedValue.Value))))
                .ToList();


            return curves.ToDictionary(t => new YieldCurveKey(Currency.Create(t.Item2.Value), t.Item1), t => GetYieldCurveSnapshot(t.Item3, tempResults));
        }

        private static ManageableYieldCurveSnapshot GetYieldCurveSnapshot(InterpolatedYieldCurveSpecification yieldCurveSpec, ViewComputationResultModel tempResults)
        {
            //TODO do yield curves only take market values?
            Func<ComputationTargetSpecification, string, bool> predicate =(cts,name)=>name == MarketValueReqName && yieldCurveSpec.ResolvedStrips.Any(s=>UniqueIdentifier.Of(s.Security) ==cts.Uid);

            var snapshotValues = GetUnstructuredSnapshot(tempResults, predicate);
            return new ManageableYieldCurveSnapshot(snapshotValues, tempResults.ValuationTime.ToDateTimeOffset());
            
        }


        private static ManageableUnstructuredMarketDataSnapshot GetUnstructuredSnapshot(ViewComputationResultModel tempResults, Func<ComputationTargetSpecification, string, bool> predicate)
        {
            var dictionary = tempResults.AllResults
                .Where(vre => predicate(vre.ComputedValue.Specification.TargetSpecification, vre.ComputedValue.Specification.ValueName))
                .ToLookup(r => r.ComputedValue.Specification.TargetSpecification)
                .ToDictionary(l => GetMarketValueSpecification(l.Key), l => (IDictionary<string, ValueSnapshot>)l.Where(ll => predicate(l.Key, ll.ComputedValue.Specification.ValueName)).ToDictionary(r => r.ComputedValue.Specification.ValueName, GetValueSnapshot))
                .Where(kvp=>kvp.Value.Any()).ToDictionary(kvp=>kvp.Key,kvp=>kvp.Value);

            return new ManageableUnstructuredMarketDataSnapshot(dictionary);
        }

       

        private static MarketDataValueSpecification GetMarketValueSpecification(ComputationTargetSpecification key)
        {
            return new MarketDataValueSpecification(GetMarketType(key.Type), key.Uid);
        }

        private static MarketDataValueType GetMarketType(ComputationTargetType type)
        {
            return type.ConvertTo<MarketDataValueType>();
        }

        private static ValueSnapshot GetValueSnapshot(ViewResultEntry entry)
        {
            return new ValueSnapshot((double) entry.ComputedValue.Value);
        }

        private static IEnumerable<ValueRequirement> GetTempView(RemoteView view, out ViewDefinition tempViewDefn)
        {
            var requiredLiveData = view.GetRequiredLiveData();

            var tempViewName = typeof(MarketDataSnapshotManager).FullName + Guid.NewGuid();

            var viewCalculationConfigurations = Enumerable.Repeat(GetMarketValuesCalculationConfiguration(requiredLiveData), 1)
                .Concat(GetYieldCurveCalculationConfigurations(view))
                ;
            
            tempViewDefn = new ViewDefinition(tempViewName, new ResultModelDefinition(ResultOutputMode.TerminalOutputs),
                view.Definition.PortfolioIdentifier,
                view.Definition.User,
                view.Definition.DefaultCurrency,
                view.Definition.MinDeltaCalcPeriod,
                view.Definition.MaxDeltaCalcPeriod,
                view.Definition.MinFullCalcPeriod,
                view.Definition.MaxFullCalcPeriod,
                viewCalculationConfigurations.ToDictionary(cc => cc.Name, cc => cc)

                );
            return requiredLiveData;
        }

        private static IEnumerable<ViewCalculationConfiguration> GetYieldCurveCalculationConfigurations(RemoteView view)
        {
            
            return view.Definition.CalculationConfigurationsByName.Values.Select(
                cc =>
                new ViewCalculationConfiguration(cc.Name,
                                                 cc.SpecificRequirements.Where(r => r.ValueName.Equals(YieldCurveValueReqName))
                                                 .Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Constraints)).ToList() 

                                                 , cc.PortfolioRequirementsBySecurityType,
                                                 cc.DefaultProperties)
                );
        }

        private static double GetValues(ViewComputationResultModel tempResults, ValueRequirement valueRequirement)
        {
            object ret;
            if (!tempResults.TryGetValue(MarketValuesConfigName, valueRequirement, out ret))
            {
                throw new ArgumentException();
            }
            return (double) ret;
        }

        private static ViewCalculationConfiguration GetMarketValuesCalculationConfiguration(IEnumerable<ValueRequirement> requiredLiveData)
        {
            return new ViewCalculationConfiguration(MarketValuesConfigName, requiredLiveData
                .ToList(), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
        }

        private static Identifier IdentifierOf(UniqueIdentifier uid)
        {
            return Identifier.Parse(uid.ToString());
        }

        protected override void Dispose(bool disposing)
        {
        	//TODO -I'm going to need thsi in order to be less slow
        }
    }
}
