using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    public class ManageableMarketDataSnapshot
    {
        private readonly UniqueIdentifier _uniqueId;

        private readonly string _basisViewName;

        private readonly ManageableUnstructuredMarketDataSnapshot _globalValues;


        private readonly Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> _yieldCurves; //TODO serialize this

        //TODO private Map<Triple<String, CurrencyUnit, CurrencyUnit>, FXVolatilitySurfaceSnapshot> _fxVolatilitySurfaces;


        public ManageableMarketDataSnapshot(string basisViewName, ManageableUnstructuredMarketDataSnapshot globalValues,
                                            Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurves,
                                            UniqueIdentifier uniqueId = null)
        {
            _basisViewName = basisViewName;
            _globalValues = globalValues;
            _yieldCurves = yieldCurves;
            _uniqueId = uniqueId;
        }


        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public string Name { get; set; }

        public Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> YieldCurves
        {
            get { return _yieldCurves; }
        }

        public string BasisViewName
        {
            get { return _basisViewName; }
        }

        public ManageableUnstructuredMarketDataSnapshot GlobalValues
        {
            get { return _globalValues; }
        }

        public IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> Values
        {
            get { return _globalValues.Values; }
        }

        public bool HaveOverrides()
        {
            return _globalValues.HaveOverrides() || _yieldCurves.Any(yc => yc.Value.HaveOverrides());
        }

        public void RemoveAllOverrides()
        {
            _globalValues.RemoveAllOverrides();

            foreach (var yieldCurveSnapshot in _yieldCurves.Values)
            {
                yieldCurveSnapshot.RemoveAllOverrides();
            }
        }

        public void UpdateFrom(ManageableMarketDataSnapshot newSnapshot)
        {
            _globalValues.UpdateFrom(newSnapshot._globalValues);

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


        public static ManageableMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc,
                                                                IFudgeDeserializer deserializer)
        {
            var uidString = ffc.GetString("uniqueId");
            UniqueIdentifier uid = uidString == null ? null : UniqueIdentifier.Parse(uidString);

            var manageableMarketDataSnapshot = new ManageableMarketDataSnapshot(
                ffc.GetString("basisViewName"),
                deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("globalValues")),
                MapBuilder.FromFudgeMsg<YieldCurveKey, ManageableYieldCurveSnapshot>(ffc, deserializer, "yieldCurves"),
                uid

                ) {Name = ffc.GetString("name")};


            return manageableMarketDataSnapshot;

        }


        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "globalValues", _globalValues);

            a.Add("name", Name);
            a.Add("basisViewName", _basisViewName);
            if (_uniqueId != null)
            {
                a.Add("uniqueId", _uniqueId);
            }
            
            a.Add("yieldCurves", MapBuilder.ToFudgeMsg(s, _yieldCurves));
        }
    }
}