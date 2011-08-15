//-----------------------------------------------------------------------
// <copyright file="ManageableVolatilitySurfaceSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl
{
    public class ManageableVolatilitySurfaceSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableVolatilitySurfaceSnapshot>
    {
        private readonly IDictionary<Pair<object, object>, ValueSnapshot> _values;

        public ManageableVolatilitySurfaceSnapshot(IDictionary<Pair<object, object>, ValueSnapshot> values)
        {
            _values = values;
        }

        public IDictionary<Pair<object, object>, ValueSnapshot> Values
        {
            get { return _values; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        public UpdateAction<ManageableVolatilitySurfaceSnapshot> PrepareUpdateFrom(ManageableVolatilitySurfaceSnapshot newObject)
        {
            var currValues = Clone(_values);
            var newValues = Clone(newObject._values);

            var valuesUpdateAction = currValues.ProjectStructure(newValues,
                                                                 PrepareUpdateFrom,
                                                                 PrepareRemoveAction,
                                                                 PrepareAddAction
                ).Aggregate(UpdateAction<ManageableVolatilitySurfaceSnapshot>.Empty, (a, b) => a.Concat(b));

            return valuesUpdateAction;
        }

        private static UpdateAction<ManageableVolatilitySurfaceSnapshot> PrepareUpdateFrom(Pair<object, object> key, ValueSnapshot currValue, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilitySurfaceSnapshot>(delegate(ManageableVolatilitySurfaceSnapshot s)
                                                                             {
                                                                                 s._values[key].MarketValue = newMarketValue;
                                                                             });
        }

        private static UpdateAction<ManageableVolatilitySurfaceSnapshot> PrepareRemoveAction(Pair<object, object> key, ValueSnapshot currValue)
        {
            return new UpdateAction<ManageableVolatilitySurfaceSnapshot>(delegate(ManageableVolatilitySurfaceSnapshot s)
                                                                             {
                                                                                 s._values.Remove(key);
                                                                             });
        }

        private static UpdateAction<ManageableVolatilitySurfaceSnapshot> PrepareAddAction(Pair<object, object> key, ValueSnapshot newValue)
        {
            var newMarketValue = newValue.MarketValue;
            return new UpdateAction<ManageableVolatilitySurfaceSnapshot>(delegate(ManageableVolatilitySurfaceSnapshot s)
                                                                             {
                                                                                 s._values.Add(key, new ValueSnapshot(newMarketValue));
                                                                             });
        }

        public bool HaveOverrides()
        {
            return _values.Any(v => v.Value.OverrideValue.HasValue);
        }

        public void RemoveAllOverrides()
        {
            foreach (var valueSnapshot in Values)
            {
                valueSnapshot.Value.OverrideValue = null;
            }
        }

        public ManageableVolatilitySurfaceSnapshot Clone()
        {
            return new ManageableVolatilitySurfaceSnapshot(Clone(_values));
        }

        private static IDictionary<T, ValueSnapshot> Clone<T>(IDictionary<T, ValueSnapshot> valueSnapshots)
        {
            //TODO dedupe
            return valueSnapshots.ToDictionary(k => k.Key, k => k.Value.Clone());
        }

        public static ManageableVolatilitySurfaceSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var values = MapBuilder.FromFudgeMsg<Pair<object, object>, ValueSnapshot>(ffc.GetMessage("values"), deserializer);

            return new ManageableVolatilitySurfaceSnapshot(values);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(ManageableVolatilitySurfaceSnapshot));

            var fudgeSerializer = new FudgeSerializer(s.Context);

            var valuesMessage = MapBuilder.ToFudgeMsg(s, _values, fudgeSerializer.SerializeToMsg, fudgeSerializer.SerializeToMsg);
            a.Add("values", valuesMessage);
        }
    }
}