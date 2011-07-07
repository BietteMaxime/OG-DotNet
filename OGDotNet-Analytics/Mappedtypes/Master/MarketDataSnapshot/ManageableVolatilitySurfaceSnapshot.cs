//-----------------------------------------------------------------------
// <copyright file="ManageableVolatilitySurfaceSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Model.Context.MarketDataSnapshot;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
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
            throw new NotImplementedException();
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