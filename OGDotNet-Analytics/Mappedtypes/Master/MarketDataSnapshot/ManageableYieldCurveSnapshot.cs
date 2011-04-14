//-----------------------------------------------------------------------
// <copyright file="ManageableYieldCurveSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;

namespace OGDotNet.Mappedtypes.master.marketdatasnapshot
{
    public class ManageableYieldCurveSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableYieldCurveSnapshot>
    {
        private readonly ManageableUnstructuredMarketDataSnapshot _values;
        private DateTimeOffset _valuationTime;

        public ManageableYieldCurveSnapshot(ManageableUnstructuredMarketDataSnapshot values, DateTimeOffset valuationTime)
        {
            _valuationTime = valuationTime;
            _values = values;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public ManageableUnstructuredMarketDataSnapshot Values
        {
            get { return _values; }
        }

        public UpdateAction PrepareUpdateFrom(ManageableYieldCurveSnapshot other)
        {
            UpdateAction valuesAction = _values.PrepareUpdateFrom(other._values);
            var timeAction = new UpdateAction(delegate
                                 {
                                     _valuationTime = other.ValuationTime;
                                     InvokePropertyChanged(new PropertyChangedEventArgs("ValuationTime"));
                                 });
            return valuesAction.Concat(timeAction);
        }

        public bool HaveOverrides()
        {
            return _values.HaveOverrides();
        }

        public void RemoveAllOverrides()
        {
            _values.RemoveAllOverrides();
        }

        public static ManageableYieldCurveSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset valuationTime = ((FudgeDateTime) ffc.GetByName("valuationTime").Value).ToDateTimeOffset();
            return new ManageableYieldCurveSnapshot(deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("values")), valuationTime);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("valuationTime", new FudgeDateTime(_valuationTime));
            s.WriteInline(a, "values", _values);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}