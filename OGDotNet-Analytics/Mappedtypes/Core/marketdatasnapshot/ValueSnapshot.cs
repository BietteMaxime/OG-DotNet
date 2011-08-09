//-----------------------------------------------------------------------
// <copyright file="ValueSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot
{
    public class ValueSnapshot : INotifyPropertyChanged
    {
        private double? _marketValue;
        private double? _overrideValue;

        public ValueSnapshot(double? marketValue)
        {
            _marketValue = marketValue;
        }

        public double? MarketValue
        {
            get
            {
                return _marketValue;
            }
            set
            {
                if (value != _marketValue)
                {
                    InvokePropertyChanged("MarketValue");
                    _marketValue = value;
                }
            }
        }

        public double? OverrideValue
        {
            get
            {
                return _overrideValue;
            }
            set
            {
                if (value != _overrideValue)
                {
                    _overrideValue = value;
                    InvokePropertyChanged("OverrideValue");
                }
            }
        }

        public ValueSnapshot Clone()
        {
            return new ValueSnapshot(MarketValue) { OverrideValue = OverrideValue};
        }

        public static ValueSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ValueSnapshot(ffc.GetDouble("marketValue")) {OverrideValue = ffc.GetDouble("overrideValue")};
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_overrideValue != null)
            {
                a.Add("overrideValue", _overrideValue);
            }
            if (_marketValue != null)
            {
                a.Add("marketValue", _marketValue);
            }
        }

        private void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        public override string ToString()
        {
            return string.Format("[ValueSnapshot {0} {1}", _marketValue, _overrideValue);
        }
    }
}
