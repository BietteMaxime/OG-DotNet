using System.ComponentModel;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class ValueSnapshot : INotifyPropertyChanged
    {
        private Identifier _security;
        public Identifier Security
        {
            get { return _security; }
            set
            {
                InvokePropertyChanged("Security");
                _security = value;
            }
        }

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
