using System;
using System.ComponentModel;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class ValueSnapshot : INotifyPropertyChanged
    {
        public Identifier Security { get; set; }

        private double _marketValue;
        public double MarketValue
        {
            get { return _marketValue; }
            set
            {
                InvokePropertyChanged("MarketValue");
                _marketValue = value;
            }
        }

        private double? _overrideValue;
        public double? OverrideValue
        {
            get { return _overrideValue; }
            set
            {
                if (value != _overrideValue)
                {
                    _overrideValue = value;
                    InvokePropertyChanged("OverrideValue");
                }
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
    }
}
