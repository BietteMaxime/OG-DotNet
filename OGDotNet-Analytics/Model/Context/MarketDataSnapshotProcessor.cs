//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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


        internal static MarketDataSnapshotProcessor Create(RemoteEngineContext context, RemoteView view, DateTimeOffset valuationTime, CancellationToken ct)
        {
            var rawMarketDataSnapper = new RawMarketDataSnapper(context, view);
            var snapshot = rawMarketDataSnapper.CreateSnapshotFromView(valuationTime, ct);
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


        public UpdateAction PrepareUpdate(CancellationToken ct = default(CancellationToken))
        {
            return PrepareUpdate(s => s, ct);
        }
        
        public UpdateAction PrepareGlobalValuesUpdate(CancellationToken ct = default(CancellationToken))
        {
            return PrepareUpdate(s => s.GlobalValues, ct);
        }

        public UpdateAction PrepareYieldCurveUpdate(YieldCurveKey yieldCurveKey, CancellationToken ct = default(CancellationToken))
        {
            if (! _snapshot.YieldCurves.ContainsKey(yieldCurveKey))
            {//TODO should I be able to add a yield curve like this?
                throw new ArgumentException(string.Format("Nonexistant yield curve {0}",yieldCurveKey));
            }

            ManageableMarketDataSnapshot newSnapshot = GetNewSnapshotForUpdate(ct);
            if (newSnapshot.YieldCurves.ContainsKey(yieldCurveKey))
            {
                return PrepareUpdate(s => s.YieldCurves[yieldCurveKey], newSnapshot);
            }
            else
            {
                return _snapshot.PrepareRemoveAction(yieldCurveKey, _snapshot.YieldCurves[yieldCurveKey]);
            }
        }

        private UpdateAction PrepareUpdate<T>(Func<ManageableMarketDataSnapshot, T> scopeSelector, CancellationToken ct = default(CancellationToken)) where T: IUpdatableFrom<T>
        {
            ManageableMarketDataSnapshot newSnapshot = GetNewSnapshotForUpdate(ct);
            return PrepareUpdate(scopeSelector, newSnapshot);
        }

        private UpdateAction PrepareUpdate<T>(Func<ManageableMarketDataSnapshot, T> scopeSelector, ManageableMarketDataSnapshot newSnapshot) where T : IUpdatableFrom<T>
        {
            return scopeSelector(Snapshot).PrepareUpdateFrom(scopeSelector(newSnapshot));
        }

        private ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct)
        {
            return _rawMarketDataSnapper.CreateSnapshotFromView(DateTimeOffset.Now, ct);
        }

        #region BuildOverridenView
        //TODO this shouldn't require so much hackery
        
        public enum ViewOptions
        {
            AllSnapshotValues,
            OverridesAndLiveValues
        }

        private class ValueReqComparer  : IEqualityComparer<ValueRequirement>
        {
            public static readonly ValueReqComparer Instance = new ValueReqComparer();
            public bool Equals(ValueRequirement x, ValueRequirement y)
            {
                return x.ValueName == y.ValueName && x.TargetSpecification.Equals(y.TargetSpecification);
            }

            public int GetHashCode(ValueRequirement obj)
            {
                return obj.TargetSpecification.GetHashCode();
            }
        }
        public RemoteView GetViewOfSnapshot(ViewOptions options)
        {
            //TODO this is a hack since I can't do structured overrides in the engine yet
            var values = 
                (_snapshot.Values.Concat(_snapshot.YieldCurves.SelectMany(y=>y.Value.Values.Values)
                ).SelectMany(kvp => kvp.Value.Select(v => Tuple.Create(GetOverrideReq(kvp.Key, v), v.Value)))
                 )
                .ToLookup(t => t.Item1, t => t.Item2, ValueReqComparer.Instance);

            Dictionary<ValueRequirement, double> overrides = GetOverrides(options, values);
            return _rawMarketDataSnapper.GetViewOfSnapshot(overrides);
        }

        private static Dictionary<ValueRequirement, double> GetOverrides(ViewOptions options, ILookup<ValueRequirement, ValueSnapshot> values)
        {
            switch (options)
            {
                case ViewOptions.AllSnapshotValues:
                    return values.ToDictionary(g => g.Key, ChoseBestOverrideValue);
                case ViewOptions.OverridesAndLiveValues:
                    var dictionary = new Dictionary<ValueRequirement, double>();
                    foreach (var group in values)
                    {
                        double value;
                        if (TryGetOverrideValue(group, out value))
                        {
                            dictionary.Add(group.Key, value);
                        }
                    }
                    return dictionary;
                default:
                    throw new ArgumentOutOfRangeException("options");
            }
        }

        private static bool TryGetOverrideValue(IEnumerable<ValueSnapshot> snapshots, out double value)
        {
            //Lots of errors here possible if there are overrides  I can't express
            if (snapshots.Any(v => v.OverrideValue.HasValue))
            {
                value = snapshots.Single(v => v.OverrideValue.HasValue).OverrideValue.Value;
                return true;
            }
            value = double.NaN;
            return false;
        }
        private static double ChoseBestOverrideValue(IEnumerable<ValueSnapshot> snapshots)
        {
            //Lots of errors here possible if there are overrides  I can't express
            double value;
            if (TryGetOverrideValue(snapshots, out value))
            {
                return value;
            }
            else
            {
                return snapshots.Select(s => s.MarketValue).Distinct().Single();
            }
        }

        #endregion
        #region YieldCurveView

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(CancellationToken ct = default(CancellationToken))
        {//TODO this is slooow and we only need to do this much work if there's awkward overrides 
            var ret = new Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>>();
            foreach (var manageableYieldCurveSnapshot in _snapshot.YieldCurves)
            {
                ct.ThrowIfCancellationRequested();
                ret.Add(manageableYieldCurveSnapshot.Key, GetYieldCurve(manageableYieldCurveSnapshot,  ct));
            }
            return ret;
        }

        public Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities> GetYieldCurve(KeyValuePair<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurveSnapshot, CancellationToken ct = default(CancellationToken))
        {
            var overrides = yieldCurveSnapshot.Value.Values.Values.
                    SelectMany(kvp =>kvp.Value.Select(v=>GetOverrideTuple(kvp,v))
                ).ToDictionary(t=>t.Item1,t=>t.Item2);

            var results = _rawMarketDataSnapper.GetAllResults(yieldCurveSnapshot.Value.ValuationTime, overrides, ct);

            ct.ThrowIfCancellationRequested();

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
            return EnumUtils<MarketDataValueType, ComputationTargetType>.ConvertTo(target.Type);
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
