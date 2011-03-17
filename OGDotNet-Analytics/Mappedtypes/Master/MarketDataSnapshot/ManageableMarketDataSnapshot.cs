using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.Id;
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class ManageableMarketDataSnapshot : MarketDataSnapshotScope
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly Dictionary<Pair<String, Currency>, YieldCurveSnapshot> _yieldCurves;//TODO serialize this

        //TODO private Map<Triple<String, CurrencyUnit, CurrencyUnit>, FXVolatilitySurfaceSnapshot> _fxVolatilitySurfaces;


        public ManageableMarketDataSnapshot(IDictionary<ComputationTargetSpecification, IDictionary<string, ValueSnapshot>> valueSnapshots, Dictionary<Pair<String, Currency>, YieldCurveSnapshot> yieldCurves, UniqueIdentifier uniqueId = null)
            : base(valueSnapshots)
        {
            _yieldCurves = yieldCurves;
            _uniqueId = uniqueId;
        }


        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public string Name { get; set; }

        public Dictionary<Pair<string, Currency>, YieldCurveSnapshot> YieldCurves
        {
            get { return _yieldCurves; }
        }

        public override bool HaveOverrides()
        {
            return base.HaveOverrides() || _yieldCurves.Any(yc => yc.Value.HaveOverrides());
        }

        public override void RemoveAllOverrides()
        {
            base.RemoveAllOverrides();

            foreach (var yieldCurveSnapshot in _yieldCurves.Values)
            {
                yieldCurveSnapshot.RemoveAllOverrides();
            }
        }

        public void UpdateFrom(ManageableMarketDataSnapshot newSnapshot)
        {
            base.UpdateFrom(newSnapshot);
            
            if (! _yieldCurves.Keys.SequenceEqual(newSnapshot._yieldCurves.Keys))
            {
                //TODO handle portfolio/view changing
                throw new NotImplementedException();
            }

            foreach (var yieldCurveSnapshot in _yieldCurves.Keys)
            {
                _yieldCurves[yieldCurveSnapshot].UpdateFrom(newSnapshot._yieldCurves[yieldCurveSnapshot]);
            }
        }


        public static ManageableMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException("Leaving this till I've worked out the structure");
        }


        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException("Leaving this till I've worked out the structure");
        }
    }
}
