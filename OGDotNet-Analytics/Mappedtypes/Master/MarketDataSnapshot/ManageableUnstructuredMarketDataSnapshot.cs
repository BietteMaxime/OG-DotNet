using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot.Warnings;
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

        public UpdateAction PrepareUpdateFrom(ManageableUnstructuredMarketDataSnapshot newSnapshot)
        {
            var currValues = GetUpdateDictionary(Values);
            var newValues = GetUpdateDictionary(newSnapshot.Values);

            return currValues.ProjectStructure(newValues,
                                     PrepareUpdateFrom,
                                     PrepareRemoveAction,
                                     PrepareAddAction
                ).Aggregate((a,b)=>a.Concat(b));
        }

        private static Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> GetUpdateDictionary(IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> values)
        {
            return new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>(
                values,
                IgnoreVersionComparer.Instance
                );
        }
        private class IgnoreVersionComparer :IEqualityComparer<MarketDataValueSpecification>
        {
            public static readonly IEqualityComparer<MarketDataValueSpecification> Instance = new IgnoreVersionComparer();

            private IgnoreVersionComparer()
            {
            }

            public bool Equals(MarketDataValueSpecification x, MarketDataValueSpecification y)
            {
                return x.Type.Equals(y.Type)
                       &&
                       x.UniqueId.ToLatest().Equals(y.UniqueId.ToLatest()); //Ignore the version info
            }

            public int GetHashCode(MarketDataValueSpecification obj)
            {
                int result = obj.Type.GetHashCode();
                result = (result * 397) ^ obj.UniqueId.ToLatest().GetHashCode(); //Ignore the version info
                return result;
            }
        }

        private UpdateAction PrepareAddAction(MarketDataValueSpecification marketDataValueSpecification,  IDictionary<string, ValueSnapshot> valueSnapshots)
        {
            return new UpdateAction(
                delegate
                    {
                        Values.Add(marketDataValueSpecification, valueSnapshots);
                        InvokePropertyChanged("Values");
                    }
                );
        }

        private UpdateAction PrepareRemoveAction(MarketDataValueSpecification marketDataValueSpecification, IDictionary<string, ValueSnapshot> valueSnapshots)
        {
            
            return new UpdateAction(
                delegate
                {
                    Values.Remove(marketDataValueSpecification);
                    InvokePropertyChanged("Values");
                },
                OverriddenSecurityDisappearingWarning.Of(marketDataValueSpecification, valueSnapshots)
                );
        }

        private UpdateAction PrepareUpdateFrom(MarketDataValueSpecification currSpec, IDictionary<string, ValueSnapshot> currValues, MarketDataValueSpecification newSpec, IDictionary<string, ValueSnapshot> newValues)
        {

            var actions = currValues.ProjectStructure(newValues,
                                                (k, a, b) => new UpdateAction(delegate { a.MarketValue = b.MarketValue; }),
                                                (k, v) => PrepareRemoveAction(currSpec, currValues, k, v),
                                                (k, v) => new UpdateAction(delegate { currValues.Add(k, v); InvokePropertyChanged("Values"); })
                );
           
            UpdateAction ret = UpdateAction.Of(actions);

            if (!currSpec.Equals(newSpec))
            {//we need to update the key, since we used a non standard comparer
                ret = ret.Concat(
                    new UpdateAction(delegate
                    {
                        if (!ReferenceEquals(Values[currSpec], currValues))
                            throw new ArgumentException();
                        Values.Remove(currSpec);
                        Values.Add(newSpec, currValues);
                        InvokePropertyChanged("Values");
                    })
                    );
            }

            return ret;
        }

        private UpdateAction PrepareRemoveAction(MarketDataValueSpecification spec, IDictionary<string, ValueSnapshot> cv, string k, ValueSnapshot v)
        {
            Action updateAction = delegate
                                      {
                                          cv.Remove(k);
                                          InvokePropertyChanged("Values");
                                      };
            return new UpdateAction(updateAction, OverriddenValueDisappearingWarning.Of(spec, k, v));
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