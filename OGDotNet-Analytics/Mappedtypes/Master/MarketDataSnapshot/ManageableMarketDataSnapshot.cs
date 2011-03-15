using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class ManageableMarketDataSnapshot : MarketDataSnapshotScope
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly Dictionary<Pair<String, Currency>, YieldCurveSnapshot> _yieldCurves;//TODO serialize this

        //TODO private Map<Triple<String, CurrencyUnit, CurrencyUnit>, FXVolatilitySurfaceSnapshot> _fxVolatilitySurfaces;


        public ManageableMarketDataSnapshot(Dictionary<Identifier, ValueSnapshot> valueSnapshots, Dictionary<Pair<String, Currency>, YieldCurveSnapshot> yieldCurves, UniqueIdentifier uniqueId = null) : base(valueSnapshots)
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
            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));

            return new ManageableMarketDataSnapshot(GetValues(ffc), null /* TODO */, uid) { Name = ffc.GetString("name") };
        }

        private static Dictionary<Identifier, ValueSnapshot> GetValues(IFudgeFieldContainer msg)
        {
            var ffc = msg.GetMessage("values");

            var ret = new Dictionary<Identifier, ValueSnapshot>();

            Identifier key = null;

            foreach (var fudgeField in ffc)
            {
                switch (fudgeField.Ordinal)
                {
                    case 1:
                        key = Identifier.Parse((string)fudgeField.Value);
                        break;
                    case 2:
                        if (key == null)
                            throw new ArgumentException();
                        var valueMessage = (IFudgeFieldContainer)fudgeField.Value;
                        var value = new ValueSnapshot
                               {
                                   MarketValue = (double)valueMessage.GetDouble("marketValue"),
                                   OverrideValue = valueMessage.GetDouble("overrideValue"),
                                   Security = Identifier.Of(valueMessage.GetString("Scheme"), valueMessage.GetString("Value"))
                               };

                        ret.Add(key, value);
                        key = null;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }


            return ret;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("name", Name);

            s.WriteInline(a, "uniqueId", UniqueId);

            WriteValues(a, Values);
        }

        private static void WriteValues(IAppendingFudgeFieldContainer a, Dictionary<Identifier, ValueSnapshot> values)
        {
            var msg = new FudgeMsg();
            foreach (var valueSnapshot in values)
            {

                msg.Add(1, valueSnapshot.Key.ToString());

                var valueMsg = new FudgeMsg();


                valueMsg.Add("marketValue", 2, valueSnapshot.Value.MarketValue);
                if (valueSnapshot.Value.OverrideValue != null)
                {
                    valueMsg.Add("overrideValue", 2, valueSnapshot.Value.OverrideValue);
                }
                valueMsg.Add("Scheme", 2, valueSnapshot.Value.Security.Scheme);
                valueMsg.Add("Value", 2, valueSnapshot.Value.Security.Value);



                msg.Add(2, valueMsg);


            }

            a.Add("values", msg);

        }
    }
}
