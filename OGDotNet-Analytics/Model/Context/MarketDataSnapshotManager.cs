using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
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


        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly MarketDataSnapshotHelper _helper;

        public MarketDataSnapshotManager(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
            _helper = new MarketDataSnapshotHelper(remoteEngineContext);
        }


        public MarketDataSnapshotProcessor GetProcessor(ManageableMarketDataSnapshot snapshot)
        {
            return new MarketDataSnapshotProcessor(_remoteEngineContext, snapshot);    
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
            ViewComputationResultModel allResults = _helper.GetAllResults(view, valuationTime);

            var requiredLiveData = view.GetRequiredLiveData();

            return new ManageableMarketDataSnapshot(view.Name, GetUnstructuredData(allResults, requiredLiveData), GetYieldCurves(allResults));
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(ViewComputationResultModel tempResults)
        {
         
            var resultsByKey = tempResults.AllResults.Where(r => r.ComputedValue.Specification.ValueName == MarketDataSnapshotHelper.YieldCurveSpecValueReqName)
                .Select(r=>(InterpolatedYieldCurveSpecificationWithSecurities) r.ComputedValue.Value)
                .ToLookup(GetYieldCurveKey,s=>s);
            
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
            IEnumerable<ValueRequirement> reqs = yieldCurveSpec.Strips.Select(s => new ValueRequirement(MarketDataSnapshotHelper.MarketValueReqName, new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Of(s.SecurityIdentifier))));
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

        
        protected override void Dispose(bool disposing)
        {
        	if (disposing)
        	{
        	    _helper.Dispose();
        	}
        }
    }
}
