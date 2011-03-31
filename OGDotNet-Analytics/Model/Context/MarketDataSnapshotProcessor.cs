using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    public class MarketDataSnapshotProcessor : DisposableBase
    {
        

        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly string _viewName;
        private readonly MarketDataSnapshotHelper _helper;


        public MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, string viewName)
        {
            _remoteEngineContext = remoteEngineContext;
            _viewName = viewName;
            _helper = new MarketDataSnapshotHelper(_remoteEngineContext);
        }


        private RemoteView View
        {
            get { return _remoteEngineContext.ViewProcessor.GetView(_viewName); }
        }
        private ViewDefinition ViewDefinition
        {
            get { return View.Definition; }
        }


        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(ManageableMarketDataSnapshot snapshot)
        {
            return GetYieldCurves(snapshot, DateTimeOffset.Now);
        }
        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(ManageableMarketDataSnapshot snapshot, DateTimeOffset valuationTime)
        {
            return snapshot.YieldCurves.ToDictionary(kvp => kvp.Key, kvp => GetYieldCurve(kvp, valuationTime));
        }

        private Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetYieldCurve(KeyValuePair<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurveSnapshot, DateTimeOffset valuationTime)
        {//TODO this is slooow
            var overrides = yieldCurveSnapshot.Value.Values.Values.
                    SelectMany(kvp =>kvp.Value.Select(v=>GetOverrideTuple(kvp,v))
                ).ToDictionary(t=>t.Item1,t=>t.Item2);

            var results = _helper.GetAllResults(View, valuationTime,overrides);

            var curveResults =
                results.AllResults
                .Where(r =>Matches(r,yieldCurveSnapshot.Key))
                .ToLookup(c => c.ComputedValue.Specification.ValueName, c => c.ComputedValue.Value)
                .ToDictionary(g=>g.Key,g=>g.First());//We can get duplicate results in different configurations

            var curve = (YieldCurve) curveResults[MarketDataSnapshotHelper.YieldCurveValueReqName];
            var spec = (InterpolatedYieldCurveSpecificationWithSecurities)curveResults[MarketDataSnapshotHelper.YieldCurveSpecValueReqName];

            return Tuple.Create(curve, spec);
        }

        private static bool Matches(ViewResultEntry r, YieldCurveKey key)
        {

            if (! r.ComputedValue.Specification.TargetSpecification.Uid.Equals(key.Currency.Identifier))
                return false;

            var curve = r.ComputedValue.Specification.Properties["Curve"];
            return curve != null && curve.SequenceEqual(new[]{key.Name});
        }

        private static Tuple<ValueRequirement,double> GetOverrideTuple(KeyValuePair<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> kvp, KeyValuePair<string, ValueSnapshot> snap)
        {
            return new Tuple<ValueRequirement, double>(
                GetOverrideReq(kvp.Key, snap),
                snap.Value.OverrideValue ?? snap.Value.MarketValue
                );
        }

        private static ValueRequirement GetOverrideReq(MarketDataValueSpecification target, KeyValuePair<string, ValueSnapshot> snap)
        {
            return new ValueRequirement(snap.Key, new ComputationTargetSpecification(ConvertToComputationTargetType(target), target.UniqueId));
        }

        private static ComputationTargetType ConvertToComputationTargetType(MarketDataValueSpecification target)
        {
            return target.Type.ConvertTo<ComputationTargetType>();
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
