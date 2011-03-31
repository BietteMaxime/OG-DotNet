using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.Common;
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
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// This class handles creating and mutating snapshots based on Views and live data
    /// 
    /// TODO: this implementation probably shouldn't be client side
    /// TODO: we fetch way more data then I think is neccesary
    /// </summary>
    public class MarketDataSnapshotManager : DisposableBase
    {
        internal const string YieldCurveValueReqName = "YieldCurve";
        internal const string YieldCurveSpecValueReqName = "YieldCurveSpec";
        private const string MarketValueReqName = "Market_Value";

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


        public ManageableMarketDataSnapshot CreateFromView(RemoteView view)
        {
            return CreateFromView(view, DateTimeOffset.Now);
        }

        private ManageableMarketDataSnapshot CreateFromView(string viewName, DateTimeOffset valuationTime)
        {
            return CreateFromView(_remoteEngineContext.ViewProcessor.GetView(viewName), valuationTime);
        }

        /// <summary>
        /// TODO Filtering
        /// </summary>
        public UpdateAction PrepareUpdateFrom(ManageableMarketDataSnapshot basis)
        {
            
            var newSnapshot = CreateFromView(basis.BasisViewName);

            return basis.PrepareUpdateFrom(newSnapshot);
        }


        public ManageableMarketDataSnapshot CreateFromView(RemoteView view, DateTimeOffset valuationTime)
        {
            view.Init();

            ViewDefinition tempViewDefn;
            IEnumerable<ValueRequirement> requiredLiveData = GetTempView(view, out tempViewDefn);

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
                        return new ManageableMarketDataSnapshot(view.Name, GetUnstructuredData(tempResults, requiredLiveData), GetYieldCurves(tempResults, view.Definition));
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tempViewDefn.Name);                
                }
            }
        }



        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(ViewComputationResultModel tempResults, ViewDefinition definition)
        {
            //TODO less insanity
            var yCurveSpecs = definition.CalculationConfigurationsByName.SelectMany(
                c =>c.Value.SpecificRequirements.Where(r => r.ValueName == YieldCurveValueReqName)
                        .Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Constraints)).Select(r => tempResults[c.Key, r])).Select(r => (InterpolatedYieldCurveSpecificationWithSecurities)r.Value);

            //TODO I shouldn't be fetching duplicate copies s/Lookup/Dictionary/
            var resultsByKey = yCurveSpecs.ToLookup(GetYieldCurveKey, r => r);
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
            
            return new ManageableYieldCurveSnapshot(values,tempResults.ValuationTime.ToDateTimeOffset());
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
                .ToLookup(r=>new MarketDataValueSpecification(GetMarketType(r.Specification.TargetSpecification.Type), r.Specification.TargetSpecification.Uid))
                .ToDictionary(r=>r.Key,r=>(IDictionary<string, ValueSnapshot>) r.ToDictionary(e=>e.Specification.ValueName, e=>new ValueSnapshot((double)e.Value)));

            return new ManageableUnstructuredMarketDataSnapshot(dict);
        }

        private static IEnumerable<ComputedValue> GetMatchingData(IEnumerable<ValueRequirement> requiredDataSet, ViewComputationResultModel tempResults)
        {
            foreach (var valueRequirement in requiredDataSet)
            {
                ComputedValue ret;
                if( tempResults.TryGetComputedValue(MarketValuesConfigName, valueRequirement, out ret))
                {//TODO what should I do if I can't get a value
                    yield return ret;
                }
            }
        }


        private static MarketDataValueType GetMarketType(ComputationTargetType type)
        {
            return type.ConvertTo<MarketDataValueType>();
        }

        private static IEnumerable<ValueRequirement> GetTempView(RemoteView view, out ViewDefinition tempViewDefn)
        {
            var requiredLiveData = view.GetRequiredLiveData();

            var tempViewName = string.Format("{0}.{1}.{2}", typeof(MarketDataSnapshotManager).Name, view.Name, Guid.NewGuid());

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
                                                 .Select(r => new ValueRequirement(YieldCurveSpecValueReqName, r.TargetSpecification, r.Constraints))

                                                 .Concat(cc.SpecificRequirements.Where(r => r.ValueName != YieldCurveValueReqName))//TODO why do I have to fetch these as well for (e.g. Single Bind view Nelson-Siegel-Svennson Bond Curve)

                                                 .ToList() 

                                                 , cc.PortfolioRequirementsBySecurityType,
                                                 cc.DefaultProperties)
                );
        }

        private static ViewCalculationConfiguration GetMarketValuesCalculationConfiguration(IEnumerable<ValueRequirement> requiredLiveData)
        {
            return new ViewCalculationConfiguration(MarketValuesConfigName, requiredLiveData
                .ToList(), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
        }

        protected override void Dispose(bool disposing)
        {
        	//TODO -I'm going to need thsi in order to be less slow
        }
    }
}
