using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
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
    public class MarketDataSnapshotProcessor : DisposableBase
    {
        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RawMarketDataSnapper _rawMarketDataSnapper;


        internal static MarketDataSnapshotProcessor Create(RemoteEngineContext context, RemoteView view, DateTimeOffset valuationTime)
        {
            var rawMarketDataSnapper = new RawMarketDataSnapper(context, view);
            var snapshot = rawMarketDataSnapper.CreateSnapshotFromView(valuationTime);
            return new MarketDataSnapshotProcessor(snapshot,rawMarketDataSnapper);
        }

        internal MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, ManageableMarketDataSnapshot snapshot):
            this(snapshot, new RawMarketDataSnapper(remoteEngineContext, remoteEngineContext.ViewProcessor.GetView(snapshot.BasisViewName)))
        {
        }

        private MarketDataSnapshotProcessor(ManageableMarketDataSnapshot snapshot, RawMarketDataSnapper rawMarketDataSnapper)
        {
            _snapshot = snapshot;
            _rawMarketDataSnapper = rawMarketDataSnapper;
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        private RemoteView View
        {
            get {return _rawMarketDataSnapper.View; }
        }


        /// <summary>
        /// TODO Filtering
        /// </summary>
        public UpdateAction PrepareUpdate()
        {
            var newSnapshot = _rawMarketDataSnapper.CreateSnapshotFromView(DateTimeOffset.Now);
            return Snapshot.PrepareUpdateFrom(newSnapshot);
        }


        #region YieldCurveView

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves()
        {
            return GetYieldCurves(DateTimeOffset.Now);
        }
        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(DateTimeOffset valuationTime)
        {//TODO this is slooow and we only need to do this much work if there's awkward overrides 
            return _snapshot.YieldCurves.ToDictionary(kvp => kvp.Key, kvp => GetYieldCurve(kvp, valuationTime));
        }

        public Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetYieldCurve(KeyValuePair<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurveSnapshot)
        {
            return GetYieldCurve(yieldCurveSnapshot, DateTimeOffset.Now);
        }
        public Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetYieldCurve(KeyValuePair<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurveSnapshot, DateTimeOffset valuationTime)
        {
            var overrides = yieldCurveSnapshot.Value.Values.Values.
                    SelectMany(kvp =>kvp.Value.Select(v=>GetOverrideTuple(kvp,v))
                ).ToDictionary(t=>t.Item1,t=>t.Item2);

            var results = _rawMarketDataSnapper.GetAllResults(valuationTime,overrides);

            var curveResults =
                results.AllResults
                .Where(r =>Matches(r,yieldCurveSnapshot.Key))
                .ToLookup(c => c.ComputedValue.Specification.ValueName, c => c.ComputedValue.Value)
                .ToDictionary(g=>g.Key,g=>g.First());//We can get duplicate results in different configurations

            var curve = (YieldCurve) curveResults[RawMarketDataSnapper.YieldCurveValueReqName];
            var spec = (InterpolatedYieldCurveSpecificationWithSecurities)curveResults[RawMarketDataSnapper.YieldCurveSpecValueReqName];

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
        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rawMarketDataSnapper.Dispose();
            }
        }
    }
}
