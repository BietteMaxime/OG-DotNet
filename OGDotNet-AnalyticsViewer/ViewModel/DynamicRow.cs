//-----------------------------------------------------------------------
// <copyright file="DynamicRow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using OGDotNet.Mappedtypes.engine.value;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public abstract class DynamicRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentDictionary<Tuple<string, string>, ConcurrentDictionary<ValueProperties, object>> _dynamicColumns = new ConcurrentDictionary<Tuple<string, string>, ConcurrentDictionary<ValueProperties, object>>();

        public object this[ColumnHeader key]
        {
            get
            {
                ConcurrentDictionary<ValueProperties, object> ret;
                if (!_dynamicColumns.TryGetValue(GetKey(key), out ret))
                {
                    return null;
                }
                foreach (var o in ret)
                {
                    if (key.RequiredConstraints.IsSatisfiedBy(o.Key))
                    { //TODO PLAT-1299: if there are multiple we should probably work out which value to use, but in theory either is fine
                        return o.Value;
                    }
                }
                return null;
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

        public void UpdateDynamicColumns(Dictionary<ColumnHeader, object> values)
        {
            foreach (var value in values)
            {
                Tuple<string, string> key = GetKey(value.Key);
                var concurrentDictionary = _dynamicColumns.GetOrAdd(key, new ConcurrentDictionary<ValueProperties, object>());

                concurrentDictionary[value.Key.RequiredConstraints] = value.Value;
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

        private static Tuple<string, string> GetKey(ColumnHeader columnHeader)
        {
            return Tuple.Create(columnHeader.Configuration, columnHeader.ValueName);
        }
    }
}