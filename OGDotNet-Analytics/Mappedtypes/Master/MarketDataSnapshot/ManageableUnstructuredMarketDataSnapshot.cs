using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
{
    public class ManageableUnstructuredMarketDataSnapshot : INotifyPropertyChanged
    {
        private readonly IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> _values;

        public ManageableUnstructuredMarketDataSnapshot(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> values)
        {
            _values = values;
        }

        public IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> Values
        {
            get { return _values; }
        }

        public bool HaveOverrides()
        {
            return _values.Any(m => m.Value.Any(v => v.Value.OverrideValue.HasValue));
        }

        public void RemoveAllOverrides()
        {
            foreach (var valueSnapshot in _values.Values.SelectMany(m => m.Values))
            {
                valueSnapshot.OverrideValue = null;
            }
        }

        public void UpdateFrom(ManageableUnstructuredMarketDataSnapshot newSnapshot)
        {
            var newValues = newSnapshot.Values;
            var current = Values;

            bool dirty= UpdateDictionaryFrom(current, newValues);
            if (dirty)
            {
                InvokePropertyChanged("Values");
            }
        }


        private static bool UpdateDictionaryFrom(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> current, IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> newValues)
        {
            return current.UpdateDictionaryFrom(newValues, (Func<IDictionary<string, ValueSnapshot>, IDictionary<string, ValueSnapshot>, bool>)UpdateDictionaryFrom);
        }


        private static bool UpdateDictionaryFrom(IDictionary<string, ValueSnapshot> current, IDictionary<string, ValueSnapshot> newValues)
        {
            return current.UpdateDictionaryFrom(newValues, (c, n) => c.MarketValue = n.MarketValue);
        }


        public IEnumerator<KeyValuePair<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }



        public static ManageableUnstructuredMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ManageableUnstructuredMarketDataSnapshot(
                MapBuilder.FromFudgeMsg<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(ffc, deserializer, "values",
                deserializer.FromField<MarketDataValueSpecification>,
                vm => MapBuilder.FromFudgeMsg((IFudgeFieldContainer)vm.Value, deserializer, km=>(string) km.Value, deserializer.FromField<ValueSnapshot>)
                )
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            Type type = typeof(ManageableUnstructuredMarketDataSnapshot);
            s.WriteTypeHeader(a, type);
            FudgeMsg valuesMessage = MapBuilder.ToFudgeMsg(s, Values);
            a.Add("values", valuesMessage);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}