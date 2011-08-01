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
using System.Windows.Data;
using OGDotNet.Mappedtypes.Engine.value;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public abstract class DynamicRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly object _lock = new object();
        private readonly Dictionary<Tuple<string, string>, Dictionary<ValueProperties, object>> _dynamicColumns = new Dictionary<Tuple<string, string>, Dictionary<ValueProperties, object>>();

        public object this[ColumnHeader key]
        {
            get
            {
                lock (_lock)
                {
                    Dictionary<ValueProperties, object> ret;
                    if (!_dynamicColumns.TryGetValue(GetKey(key), out ret))
                    {
                        return null;
                    }
                
                    foreach (var o in ret)
                    {
                        if (key.RequiredConstraints.IsSatisfiedBy(o.Key))
                        {
                            //TODO PLAT-1299: if there are multiple we should probably work out which value to use, but in theory either is fine
                            return o.Value;
                        }
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
            lock (_lock)
            {
                foreach (var value in values)
                {
                    Tuple<string, string> key = GetKey(value.Key);

                    Dictionary<ValueProperties, object> dict;
                    if (! _dynamicColumns.TryGetValue(key, out dict))
                    {
                        dict = new Dictionary<ValueProperties, object>(1);
                        _dynamicColumns.Add(key, dict);
                    }

                    dict[value.Key.RequiredConstraints] = value.Value;
                }
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