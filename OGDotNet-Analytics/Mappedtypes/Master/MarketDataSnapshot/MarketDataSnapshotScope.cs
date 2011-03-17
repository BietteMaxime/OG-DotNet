using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public abstract class MarketDataSnapshotScope
    {
        private readonly IDictionary<ComputationTargetSpecification, IDictionary<string,ValueSnapshot>> _values;

        protected MarketDataSnapshotScope(IDictionary<ComputationTargetSpecification, IDictionary<string, ValueSnapshot>> values)
        {
            _values = values;
        }

        public IDictionary<ComputationTargetSpecification, IDictionary<string,ValueSnapshot>> Values
        {
            get { return _values; }
        }

        public virtual bool HaveOverrides()
        {
            return _values.Any(m => m.Value.Any(v=>v.Value.OverrideValue.HasValue));
        }

        public virtual void RemoveAllOverrides()
        {
            foreach (var valueSnapshot in _values.Values.SelectMany(m => m.Values))
            {
                valueSnapshot.OverrideValue = null;
            }
        }

        protected void UpdateFrom(MarketDataSnapshotScope newSnapshot)
        {
            var newValues = newSnapshot.Values;
            var current = Values;

            UpdateDictionaryFrom(current, newValues);
        }


        private static void UpdateDictionaryFrom(IDictionary<ComputationTargetSpecification, IDictionary<string, ValueSnapshot>> current, IDictionary<ComputationTargetSpecification, IDictionary<string, ValueSnapshot>> newValues)
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

    }
}