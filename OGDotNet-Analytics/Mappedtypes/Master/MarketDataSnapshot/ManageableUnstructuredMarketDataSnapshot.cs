using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
{
    public class ManageableUnstructuredMarketDataSnapshot
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

            UpdateDictionaryFrom(current, newValues);
        }


        private static void UpdateDictionaryFrom(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> current, IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> newValues)
        {
            UpdateDictionaryFrom(current, newValues, UpdateDictionaryFrom);
        }


        private static void UpdateDictionaryFrom(IDictionary<string, ValueSnapshot> current, IDictionary<string, ValueSnapshot> newValues)
        {
            UpdateDictionaryFrom(current, newValues, (c, n) => c.MarketValue = n.MarketValue);
        }


        private static void UpdateDictionaryFrom<TKey, TValueA, TValueB>(IDictionary<TKey, TValueA> dictA, IDictionary<TKey, TValueB> dictB, Action<TValueA, TValueB> updater)
        {
            CheckNoChanges(dictA, dictB);

            var enumerable = dictA.Join(dictB, a => a.Key, b => b.Key, (a, b) => Tuple.Create(a.Value, b.Value));
            foreach (var tuple in enumerable)
            {
                updater(tuple.Item1, tuple.Item2);
            }
        }

        private static void CheckNoChanges<TKey, TValueA, TValueB>(IDictionary<TKey, TValueA> current, IDictionary<TKey, TValueB> newValues)
        {
            var toAdd = newValues.Keys.Except(current.Keys).ToList();
            var toRemove = current.Keys.Except(newValues.Keys).ToList();

            if (toAdd.Any() || toRemove.Any())
            {//TODO handle dictionary changing
                throw new NotImplementedException();
            }
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
                vm => MapBuilder.FromFudgeMsg<string, ValueSnapshot>((IFudgeFieldContainer)vm.Value, deserializer, km=>(string) km.Value, deserializer.FromField<ValueSnapshot>)
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
    }
}