using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public abstract class DynamicRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentDictionary<string, object> _dynamicColumns = new ConcurrentDictionary<string, object>();

        public object this[string key]
        {
            get
            {
                object value;
                if (_dynamicColumns.TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }

        private void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        public void UpdateDynamicColumns(Dictionary<string, object> values)
        {
            foreach (var value in values)
            {
                _dynamicColumns[value.Key] = value.Value;
            }
            
            switch (values.Count)
            {
                case 0:
                    break;
                default:
                    // TODO : if there's a small number of properties changed update individual ones
                    InvokePropertyChanged(Binding.IndexerName);
                    break;
            }
        }
    }
}