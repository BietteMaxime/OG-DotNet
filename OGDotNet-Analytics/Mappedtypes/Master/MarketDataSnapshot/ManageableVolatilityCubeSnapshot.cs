//-----------------------------------------------------------------------
// <copyright file="ManageableVolatilityCubeSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
{
    public class ManageableVolatilityCubeSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableVolatilityCubeSnapshot>
    {
        private readonly Dictionary<VolatilityPoint, ValueSnapshot> _values;

        public ManageableVolatilityCubeSnapshot()
        {
            _values = new Dictionary<VolatilityPoint, ValueSnapshot>();
        }

        private ManageableVolatilityCubeSnapshot(Dictionary<VolatilityPoint, ValueSnapshot> values)
        {
            _values = values;
        }

        public Dictionary<VolatilityPoint, ValueSnapshot> Values
        {
            get { return _values.ToDictionary(k => k.Key, k => k.Value); }
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
            throw new NotImplementedException();
        }

        public static ManageableVolatilityCubeSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ManageableVolatilityCubeSnapshot(MapBuilder.FromFudgeMsg<VolatilityPoint, ValueSnapshot>(ffc.GetMessage("values"), deserializer));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(ManageableVolatilityCubeSnapshot));
            var valuesMessage = MapBuilder.ToFudgeMsg(s, _values);
            a.Add("values", valuesMessage);
        }
    }
}