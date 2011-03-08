using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class ManageableMarketDataSnapshot
    {

        private UniqueIdentifier _uniqueId;

        private String _name;

        private Dictionary<UniqueIdentifier, ValueSnapshot> _values;

        //TODO private Dictionary<Pair<String, CurrencyUnit>, YieldCurveSnapshot> _yieldCurves;

        //TODO private Map<Triple<String, CurrencyUnit, CurrencyUnit>, FXVolatilitySurfaceSnapshot> _fxVolatilitySurfaces;

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Dictionary<UniqueIdentifier, ValueSnapshot> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public static ManageableMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));

            return new ManageableMarketDataSnapshot
                       {
                           Name = ffc.GetString("name"),
                           Values = GetValues(ffc),
                           UniqueId = uid
                       };
        }

        private static Dictionary<UniqueIdentifier, ValueSnapshot> GetValues(IFudgeFieldContainer msg)
        {
            var ffc = msg.GetMessage("values");

            var ret = new Dictionary<UniqueIdentifier, ValueSnapshot>();

            UniqueIdentifier key = null;

            foreach (var fudgeField in ffc)
            {
                switch (fudgeField.Ordinal)
                {
                    case 1:
                        key = UniqueIdentifier.Parse((string)fudgeField.Value);
                        break;
                    case 2:
                        if (key == null)
                            throw new ArgumentException();
                        var valueMessage = (IFudgeFieldContainer)fudgeField.Value;
                        var value = new ValueSnapshot
                               {
                                   MarketValue = (double)valueMessage.GetDouble("marketValue"),
                                   OverrideValue = valueMessage.GetDouble("overrideValue"),
                                   Security = UniqueIdentifier.Of(valueMessage.GetString("Scheme"), valueMessage.GetString("Value"))
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

        private static void WriteValues(IAppendingFudgeFieldContainer a, Dictionary<UniqueIdentifier, ValueSnapshot> values)
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
