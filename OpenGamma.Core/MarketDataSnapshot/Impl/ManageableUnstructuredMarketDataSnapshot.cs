// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageableUnstructuredMarketDataSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Model;
using OpenGamma.Model.Context.MarketDataSnapshot;
using OpenGamma.Model.Context.MarketDataSnapshot.Warnings;
using OpenGamma.Util;

namespace OpenGamma.MarketDataSnapshot.Impl
{
    public class ManageableUnstructuredMarketDataSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableUnstructuredMarketDataSnapshot>
    {
        private readonly IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> _values;

        public ManageableUnstructuredMarketDataSnapshot(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> values)
        {
            _values = new ConcurrentDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(values);
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

        public UpdateAction<ManageableUnstructuredMarketDataSnapshot> PrepareUpdateFrom(ManageableUnstructuredMarketDataSnapshot newSnapshot)
        {
            IEqualityComparer<MarketDataValueSpecification> comparer = CreateUpdateComparer(_values, newSnapshot.Values);

            var currValues = new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(_values, comparer);
            var newValues = new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(newSnapshot.Values, comparer);

            return currValues.ProjectStructure(newValues, 
                                               PrepareUpdateFrom, 
                                               PrepareRemoveAction, 
                                               PrepareAddAction
                ).Aggregate(UpdateAction<ManageableUnstructuredMarketDataSnapshot>.Empty, (a, b) => a.Concat(b));
        }

        private static IEqualityComparer<MarketDataValueSpecification> CreateUpdateComparer(params IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>[] values)
        {
            // LAP-30
            var exclusions = new HashSet<ObjectId>();
            foreach (var value in values)
            {
                IEnumerable<ObjectId> exclusionSet = CreateExclusionSet(value);
                foreach (var objectID in exclusionSet)
                {
                    exclusions.Add(objectID);
                }
            }
            
            return IgnoreVersionComparer.Create(exclusions);
        }

        private static IEnumerable<ObjectId> CreateExclusionSet(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> values)
        {
            // LAP-30
            var excluded = new HashSet<ObjectId>();
            var seen = new HashSet<ObjectId>();
            foreach (var value in values.Keys)
            {
                ObjectId objectID = value.UniqueId.ObjectId;    
                if (!seen.Add(objectID))
                {
                    excluded.Add(objectID);
                }
            }

            return excluded;
        }

        private class IgnoreVersionComparer : IEqualityComparer<MarketDataValueSpecification>
        {
            private readonly HashSet<ObjectId> _exclusionsSet;

            /// <param name="exclusions">Those UniqueIds for which version should be respected</param>
            /// <returns></returns>
            public static IEqualityComparer<MarketDataValueSpecification> Create(IEnumerable<ObjectId> exclusions)
            {
                var exclusionsSet = new HashSet<ObjectId>(exclusions);
                return new IgnoreVersionComparer(exclusionsSet);
            }

            private IgnoreVersionComparer(HashSet<ObjectId> exclusionsSet)
            {
                _exclusionsSet = exclusionsSet;
            }

            public bool Equals(MarketDataValueSpecification x, MarketDataValueSpecification y)
            {
                if (!x.Type.Equals(y.Type))
                {
                    return false;
                }

                // NOTE: Don't need to check y here
                if (_exclusionsSet.Contains(x.UniqueId.ObjectId))
                {
                    return x.UniqueId.Equals(y.UniqueId);
                }
                else
                {
                    return x.UniqueId.ToLatest().Equals(y.UniqueId.ToLatest()); // Ignore the version info
                }
            }

            public int GetHashCode(MarketDataValueSpecification obj)
            {
                int result = obj.Type.GetHashCode();
                result = (result * 397) ^ obj.UniqueId.ToLatest().GetHashCode(); // Ignore the version info, inefficient in LAP-30, but that's rare
                return result;
            }
        }

        private static UpdateAction<ManageableUnstructuredMarketDataSnapshot> PrepareAddAction(MarketDataValueSpecification marketDataValueSpecification, IDictionary<string, ValueSnapshot> valueSnapshots)
        {
            var clonedValues = Clone(valueSnapshots);
            return new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(
                delegate(ManageableUnstructuredMarketDataSnapshot snap)
                    {
                        snap.Values.Add(marketDataValueSpecification, Clone(clonedValues));
                        snap.InvokePropertyChanged("Values");
                    }

                );
        }

        private static IDictionary<T, ValueSnapshot> Clone<T>(IDictionary<T, ValueSnapshot> valueSnapshots)
        {
            return valueSnapshots.ToDictionary(k => k.Key, k => k.Value.Clone());
        }

        private static UpdateAction<ManageableUnstructuredMarketDataSnapshot> PrepareRemoveAction(MarketDataValueSpecification marketDataValueSpecification, IDictionary<string, ValueSnapshot> valueSnapshots)
        {
            return new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(
                delegate(ManageableUnstructuredMarketDataSnapshot snap)
                    {
                        snap.Values.Remove(marketDataValueSpecification);
                        snap.InvokePropertyChanged("Values");
                    }, 
                OverriddenSecurityDisappearingWarning.Of(marketDataValueSpecification, valueSnapshots)
                );
        }

        private static UpdateAction<ManageableUnstructuredMarketDataSnapshot> PrepareUpdateFrom(MarketDataValueSpecification currSpec, IDictionary<string, ValueSnapshot> currValues, MarketDataValueSpecification newSpec, IDictionary<string, ValueSnapshot> newValues)
        {
            var actions = currValues.ProjectStructure(newValues, 
                                                      (k, a, b) =>
                                                          {
                                                              var newMarketValue = b.MarketValue;
                                                              return new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(delegate(ManageableUnstructuredMarketDataSnapshot s)
                                                                                                                                    {
                                                                                                                                        s._values[currSpec][k].MarketValue = newMarketValue;
                                                                                                                                    });
                                                          }, 
                                                      (k, v) => PrepareRemoveAction(currSpec, k, v), 
                                                      (k, v) =>
                                                          {
                                                              var valueSnapshot = v.Clone();
                                                              return new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(
                                                                  delegate(ManageableUnstructuredMarketDataSnapshot s)
                                                                      {
                                                                          s._values[currSpec].Add(k, valueSnapshot.Clone());
                                                                          s.InvokePropertyChanged("Values");
                                                                      });
                                                          });

            UpdateAction<ManageableUnstructuredMarketDataSnapshot> ret = UpdateAction<ManageableUnstructuredMarketDataSnapshot>.Create(actions);

            if (!currSpec.Equals(newSpec))
            {// we need to update the key, since we used a non standard comparer
                ret = ret.Concat(
                    new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(delegate(ManageableUnstructuredMarketDataSnapshot s)
                                                                                   {
                                                                                       var prevValue = s._values[currSpec];
                                                                                       s.Values[newSpec] = prevValue;
                                                                                       s._values.Remove(currSpec);
                                                                                       s.InvokePropertyChanged("Values");
                                                                                   })
                    );
            }

            return ret;
        }

        private static UpdateAction<ManageableUnstructuredMarketDataSnapshot> PrepareRemoveAction(MarketDataValueSpecification spec, string k, ValueSnapshot v)
        {
            Action<ManageableUnstructuredMarketDataSnapshot> updateAction = delegate(ManageableUnstructuredMarketDataSnapshot s)
                                                                                {
                                                                                    if (!s._values[spec].Remove(k))
                                                                                    {
                                                                                        throw new InvalidOperationException("Unexpected missing key");
                                                                                    }

                                                                                    s.InvokePropertyChanged("Values");
                                                                                };
            return new UpdateAction<ManageableUnstructuredMarketDataSnapshot>(updateAction, OverriddenValueDisappearingWarning.Of(spec, k, v));
        }

        public IEnumerator<KeyValuePair<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public static ManageableUnstructuredMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var dictionary = new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>();
            foreach (var entryField in ffc.GetAllByOrdinal(1))
            {
                var entryMsg = (IFudgeFieldContainer)entryField.Value;
                MarketDataValueSpecification valueSpec = null;
                string valueName = null;
                ValueSnapshot value = null;

                foreach (var field in entryMsg)
                {
                    switch (field.Name)
                    {
                        case "valueSpec":
                            valueSpec = deserializer.FromField<MarketDataValueSpecification>(field);
                            break;
                        case "valueName":
                            valueName = (string)field.Value;
                            break;
                        case "value":
                            value = deserializer.FromField<ValueSnapshot>(field);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                IDictionary<string, ValueSnapshot> innerDict;
                if (!dictionary.TryGetValue(valueSpec, out innerDict))
                {
                    innerDict = new Dictionary<string, ValueSnapshot>();
                    dictionary.Add(valueSpec, innerDict);
                }

                innerDict.Add(valueName, value);
            }

            return new ManageableUnstructuredMarketDataSnapshot(dictionary);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            Type type = typeof(ManageableUnstructuredMarketDataSnapshot);
            s.WriteTypeHeader(a, type);
            foreach (var value in Values)
            {
                foreach (var valueSnapshot in value.Value)
                {
                    var openGammaFudgeContext = (OpenGammaFudgeContext)s.Context;
                    var newMessage = s.Context.NewMessage();
                    newMessage.Add("valueSpec", openGammaFudgeContext.GetSerializer().SerializeToMsg(value.Key));
                    newMessage.Add("valueName", valueSnapshot.Key);
                    newMessage.Add("value", openGammaFudgeContext.GetSerializer().SerializeToMsg(valueSnapshot.Value));
                    a.Add(1, newMessage);
                }
            }
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

        public ManageableUnstructuredMarketDataSnapshot Clone()
        {
            return new ManageableUnstructuredMarketDataSnapshot(Values.ToDictionary(k => k.Key, k => Clone(k.Value)));
        }

        public void Add(MarketDataValueSpecification spec, string valueName)
        {
            IDictionary<string, ValueSnapshot> entry;
            if (!Values.TryGetValue(spec, out entry))
            {
                entry = new Dictionary<string, ValueSnapshot>();
                Values.Add(spec, entry);
            }

            entry.Add(valueName, new ValueSnapshot(null));
            InvokePropertyChanged("Values");
        }
    }
}