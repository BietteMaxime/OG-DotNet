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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl
{
    public class ManageableVolatilityCubeSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableVolatilityCubeSnapshot>
    {
        private readonly IDictionary<VolatilityPoint, ValueSnapshot> _values;
        private readonly ManageableUnstructuredMarketDataSnapshot _otherValues;
        private readonly IDictionary<Pair<Tenor, Tenor>, ValueSnapshot> _strikes;
        public ManageableVolatilityCubeSnapshot(ManageableUnstructuredMarketDataSnapshot otherValues)
        {
            ArgumentChecker.NotNull(otherValues, "otherValues");
            _values = new Dictionary<VolatilityPoint, ValueSnapshot>();
            _otherValues = otherValues;
            _strikes = new Dictionary<Pair<Tenor, Tenor>, ValueSnapshot>();
        }

        private ManageableVolatilityCubeSnapshot(IDictionary<VolatilityPoint, ValueSnapshot> values, ManageableUnstructuredMarketDataSnapshot otherValues, IDictionary<Pair<Tenor, Tenor>, ValueSnapshot> strikes)
        {
            _values = values;
            _otherValues = otherValues;
            _strikes = strikes;
        }

        public Dictionary<VolatilityPoint, ValueSnapshot> Values
        {
            get { return _values.ToDictionary(k => k.Key, k => k.Value); }
        }

        public ManageableUnstructuredMarketDataSnapshot OtherValues
        {
            get { return _otherValues; }
        }

        public IDictionary<Pair<Tenor, Tenor>, ValueSnapshot> Strikes
        {
            get { return _strikes; }
        }

        public void SetPoint(VolatilityPoint point, ValueSnapshot snapshot)
        {
            _values[point] = snapshot;
            InvokePropertyChanged(new PropertyChangedEventArgs("Values"));
        }

        public void SetStrike(Pair<Tenor, Tenor> key, ValueSnapshot valueSnapshot)
        {
            _strikes[key] = valueSnapshot;
            InvokePropertyChanged(new PropertyChangedEventArgs("Strikes"));
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
            var newValues = Clone(newObject._values);

            var valuesUpdateAction = currValues.ProjectStructure(newValues,
                                                                 PrepareUpdateFrom,
                                                                 PrepareRemoveAction,
                                                                 PrepareAddAction
                ).Aggregate(UpdateAction<ManageableVolatilityCubeSnapshot>.Empty, (a, b) => a.Concat(b));

            var strikesAction = Clone(_strikes).ProjectStructure(Clone(newObject._strikes), 
                PrepareUpdateFrom,
                                                                 PrepareRemoveAction,
                                                                 PrepareAddAction).Aggregate(UpdateAction<ManageableVolatilityCubeSnapshot>.Empty, (a, b) => a.Concat(b));
            return valuesUpdateAction.Concat(updateAction).Concat(strikesAction);
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

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareUpdateFrom(Pair<Tenor, Tenor> key, ValueSnapshot currValue, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
            {
                s._strikes[key].MarketValue = newMarketValue;
            });
        }

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareRemoveAction(Pair<Tenor, Tenor> key, ValueSnapshot currValue)
        {
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
            {
                s._strikes.Remove(key);
            });
        }

        private static UpdateAction<ManageableVolatilityCubeSnapshot> PrepareAddAction(Pair<Tenor, Tenor> key, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilityCubeSnapshot>(delegate(ManageableVolatilityCubeSnapshot s)
            {
                s._strikes.Add(key, new ValueSnapshot(newMarketValue));
            });
        }

        public ManageableVolatilityCubeSnapshot Clone()
        {
            return new ManageableVolatilityCubeSnapshot(Clone(_values), _otherValues.Clone(), Clone(_strikes));
        }

        public bool HaveOverrides()
        {
            return _otherValues.HaveOverrides() 
                   || _values.Any(v => v.Value != null && v.Value.OverrideValue != null)
                   || _strikes.Any(v => v.Value != null && v.Value.OverrideValue != null);
        }

        public void RemoveAllOverrides()
        {
            _otherValues.RemoveAllOverrides();
            foreach (var valueSnapshot in Values)
            {
                if (valueSnapshot.Value != null && valueSnapshot.Value.OverrideValue != null)
                {
                    valueSnapshot.Value.OverrideValue = null;
                }
            }
            foreach (var valueSnapshot in _strikes)
            {
                if (valueSnapshot.Value != null && valueSnapshot.Value.OverrideValue != null)
                {
                    valueSnapshot.Value.OverrideValue = null;
                }
            }
        }

        private static IDictionary<T, ValueSnapshot> Clone<T>(IDictionary<T, ValueSnapshot> valueSnapshots)
        {
            return valueSnapshots.ToDictionary(k => k.Key, k => k.Value.Clone());
        }

        public static ManageableVolatilityCubeSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var values = MapBuilder.FromFudgeMsg<VolatilityPoint, ValueSnapshot>(ffc.GetMessage("values"), deserializer);
            var othervalues = deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("otherValues"));
            var strikesMessage = ffc.GetMessage("strikes");

            Dictionary<Pair<Tenor, Tenor>, ValueSnapshot> strikes;
            if (strikesMessage == null) 
            {
                strikes = new Dictionary<Pair<Tenor, Tenor>, ValueSnapshot>();
            }
            else
            {
                strikes = MapBuilder.FromFudgeMsg(strikesMessage, DeserializeKey, deserializer.FromField<ValueSnapshot>);
            }
            return new ManageableVolatilityCubeSnapshot(values, othervalues, strikes);
        }

        private static Pair<Tenor, Tenor> DeserializeKey(IFudgeField fudgeField)
        {
            Tenor first = DeserializeTenor(fudgeField, "first");
            Tenor second = DeserializeTenor(fudgeField, "second");
            return new Pair<Tenor, Tenor>(first, second);
        }

        private static Tenor DeserializeTenor(IFudgeField fudgeField, string fieldName)
        {
            var fudgeFieldContainer = ((IFudgeFieldContainer) fudgeField.Value).GetMessage(fieldName);
            var value = fudgeFieldContainer.GetString("tenor");
            return new Tenor(value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(ManageableVolatilityCubeSnapshot));
            var valuesMessage = MapBuilder.ToFudgeMsg(s, _values);
            a.Add("values", valuesMessage);
            s.WriteInline(a, "otherValues", _otherValues);
            var strikesMessage = MapBuilder.ToFudgeMsg(s, _strikes);
            a.Add("strikes", strikesMessage);
        }
    }
}