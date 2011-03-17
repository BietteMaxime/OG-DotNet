using System.ComponentModel;
using OGDotNet.Mappedtypes.engine;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class ValueSnapshot : INotifyPropertyChanged
    {
        private readonly ComputationTargetSpecification _computationTarget;
        private readonly string _valueName;
        private double _marketValue;
        private double? _overrideValue;

        public ValueSnapshot(ComputationTargetSpecification computationTarget, string valueName, double marketValue)
        {
            _computationTarget = computationTarget;
            _valueName = valueName;
            _marketValue = marketValue;
        }

        public ComputationTargetSpecification ComputationTarget
        {
            get { return _computationTarget; }
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public double MarketValue
        {
            get { return _marketValue; }
            set
            {
                InvokePropertyChanged("MarketValue");
                _marketValue = value;
            }
        }

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
