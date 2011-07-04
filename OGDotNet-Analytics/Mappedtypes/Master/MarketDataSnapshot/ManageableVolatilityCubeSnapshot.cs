//-----------------------------------------------------------------------
// <copyright file="ManageableVolatilityCubeSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
{
    public class ManageableVolatilityCubeSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableVolatilityCubeSnapshot>
    {
        private readonly IDictionary<VolatilityPoint, ValueSnapshot> _values;
        private readonly ManageableUnstructuredMarketDataSnapshot _otherValues;

        public ManageableVolatilityCubeSnapshot(ManageableUnstructuredMarketDataSnapshot otherValues)
        {
            _values = new Dictionary<VolatilityPoint, ValueSnapshot>();
            _otherValues = otherValues;
        }

        private ManageableVolatilityCubeSnapshot(IDictionary<VolatilityPoint, ValueSnapshot> values, ManageableUnstructuredMarketDataSnapshot otherValues)
        {
            _values = values;
            _otherValues = otherValues;
        }

        public Dictionary<VolatilityPoint, ValueSnapshot> Values
        {
            get { return _values.ToDictionary(k => k.Key, k => k.Value); }
        }

        public ManageableUnstructuredMarketDataSnapshot OtherValues
        {
            get { return _otherValues; }
        }

        public void SetPoint(VolatilityPoint point, ValueSnapshot snapshot)
        {
            _values[point] = snapshot;
            InvokePropertyChanged(new PropertyChangedEventArgs("Values"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        public UpdateAction<ManageableVolatilityCubeSnapshot> PrepareUpdateFrom(ManageableVolatilityCubeSnapshot newObject)
        {
            var otherValuesAction = _otherValues.PrepareUpdateFrom(newObject._otherValues);
            var updateAction = otherValuesAction.Wrap<ManageableVolatilityCubeSnapshot>(y => y._otherValues);

            var currValues = Clone(_values);
            var newValues = Clone(_values);

            var valuesUpdateAction = currValues.ProjectStructure(newValues,
                                                                PrepareUpdateFrom,
                                                                PrepareRemoveAction,
                                                                PrepareAddAction
                ).Aggregate(UpdateAction<ManageableVolatilityCubeSnapshot>.Empty, (a, b) => a.Concat(b));

            return valuesUpdateAction.Concat(updateAction);
        }

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareUpdateFrom(VolatilityPoint key, ValueSnapshot currValue, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
                                                                                  {
                                                                                      s._values[key].MarketValue = newMarketValue;
                                                                                  });
        }

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareRemoveAction(VolatilityPoint key, ValueSnapshot currValue)
        {
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
                                                                          {
                                                                              s._values.Remove(key);
                                                                          });
        }

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareAddAction(VolatilityPoint key, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
            {
                s._values.Add(key, new ValueSnapshot(newMarketValue));
            });
        }

        public ManageableVolatilityCubeSnapshot Clone()
        {
            return new ManageableVolatilityCubeSnapshot(Clone(_values), _otherValues.Clone());
        }

        public bool HaveOverrides()
        {
            return _otherValues.HaveOverrides() || _values.Any(v => v.Value != null && v.Value.OverrideValue != null);
        }

        private static IDictionary<T, ValueSnapshot> Clone<T>(IDictionary<T, ValueSnapshot> valueSnapshots)
        {
            return valueSnapshots.ToDictionary(k => k.Key, k => k.Value.Clone());
        }

        public static ManageableVolatilityCubeSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ManageableVolatilityCubeSnapshot(MapBuilder.FromFudgeMsg<VolatilityPoint, ValueSnapshot>(ffc.GetMessage("values"), deserializer),
                deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("otherValues")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(ManageableVolatilityCubeSnapshot));
            var valuesMessage = MapBuilder.ToFudgeMsg(s, _values);
            a.Add("values", valuesMessage);
            s.WriteInline(a, "otherValues", _otherValues);
        }
    }
}