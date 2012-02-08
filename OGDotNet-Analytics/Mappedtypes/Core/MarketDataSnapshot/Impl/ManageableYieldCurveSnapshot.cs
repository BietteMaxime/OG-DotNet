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
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl
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

        public UpdateAction<ManageableYieldCurveSnapshot> PrepareUpdateFrom(ManageableYieldCurveSnapshot other)
        {
            var valuesAction = _values.PrepareUpdateFrom(other._values);
            var otherValTime = other.ValuationTime;

            var timeAction = new UpdateAction<ManageableYieldCurveSnapshot>(delegate(ManageableYieldCurveSnapshot snap)
                                                                                {
                                                                                    snap._valuationTime = otherValTime;
                                                                                    snap.InvokePropertyChanged(new PropertyChangedEventArgs("ValuationTime"));
                                                                                });
            return valuesAction.Wrap<ManageableYieldCurveSnapshot>(y => y._values).Concat(timeAction);
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
            var valuationTime = ffc.GetValue<FudgeDateTime>("valuationTime").ToDateTimeOffsetWithDefault();
            return new ManageableYieldCurveSnapshot(deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("values")), valuationTime);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("valuationTime", _valuationTime);
            s.WriteInline(a, "values", _values);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        public ManageableYieldCurveSnapshot Clone()
        {
            return new ManageableYieldCurveSnapshot(Values.Clone(), ValuationTime);
        }
    }
}