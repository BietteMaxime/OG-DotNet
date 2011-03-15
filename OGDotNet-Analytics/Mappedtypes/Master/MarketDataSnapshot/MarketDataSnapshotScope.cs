using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public abstract class MarketDataSnapshotScope
    {
        private readonly Dictionary<Identifier, ValueSnapshot> _values;

        protected MarketDataSnapshotScope(Dictionary<Identifier, ValueSnapshot> values)
        {
            _values = values;
        }

        public Dictionary<Identifier, ValueSnapshot> Values
        {
            get { return _values; }
        }

        public virtual bool HaveOverrides()
        {
            return _values.Any(v => v.Value.OverrideValue.HasValue);
        }

        public virtual void RemoveAllOverrides()
        {
            foreach (var valueSnapshot in _values.Values)
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


        private static void UpdateDictionaryFrom(Dictionary<Identifier, ValueSnapshot> current, Dictionary<Identifier, ValueSnapshot> newValues)
        {
            foreach (var valueSnapshot in current)
            {
                ValueSnapshot newValue;
                if (newValues.TryGetValue(valueSnapshot.Key, out newValue))
                {
                    valueSnapshot.Value.MarketValue = newValue.MarketValue;
                }
            }

            var toAdd = newValues.Keys.Except(current.Keys).ToList();
            var toRemove = current.Keys.Except(newValues.Keys).ToList();

            if (toAdd.Any() || toRemove.Any())
            {//TODO handle dictionary changing
                throw new NotImplementedException();
            }
        }
    }
}