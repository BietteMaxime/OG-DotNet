//-----------------------------------------------------------------------
// <copyright file="ManageableMarketDataSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot.Warnings;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    public class ManageableMarketDataSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableMarketDataSnapshot>
    {
        private readonly string _basisViewName;

        private readonly ManageableUnstructuredMarketDataSnapshot _globalValues;


        private readonly Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> _yieldCurves;

        //TODO private Map<Triple<string, CurrencyUnit, CurrencyUnit>, FXVolatilitySurfaceSnapshot> _fxVolatilitySurfaces;

        private UniqueIdentifier _uniqueId;

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
            set { _uniqueId = value; }
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

        public UpdateAction PrepareUpdateFrom(ManageableMarketDataSnapshot newSnapshot)
        {
            UpdateAction globalUpdate = _globalValues.PrepareUpdateFrom(newSnapshot._globalValues);

            var ycActions = _yieldCurves.ProjectStructure(newSnapshot._yieldCurves,
                (k, va, vb) => va.PrepareUpdateFrom(vb),
                PrepareRemoveAction,
                PrepareAddAction
                );

            return globalUpdate.Concat(UpdateAction.Of(ycActions));
        }

        internal UpdateAction PrepareRemoveAction(YieldCurveKey key, ManageableYieldCurveSnapshot value)
        {
            return new UpdateAction(
                delegate
                {
                    _yieldCurves.Remove(key);
                    InvokePropertyChanged(new PropertyChangedEventArgs("YieldCurves"));
                },
                    OverriddenYieldCurveDisappearingWarning.Of(key, value)
                );
        }

        private UpdateAction PrepareAddAction(YieldCurveKey key, ManageableYieldCurveSnapshot value)
        {
            return new UpdateAction(
               delegate
               {
                   _yieldCurves.Add(key, value);
                   InvokePropertyChanged(new PropertyChangedEventArgs("YieldCurves"));
               }
               );
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

                ) { Name = ffc.GetString("name") };


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

        public event PropertyChangedEventHandler PropertyChanged;


        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}