//-----------------------------------------------------------------------
// <copyright file="PrimitiveRow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PrimitiveRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private readonly UniqueIdentifier _targetId;
        private Dictionary<string, object> _columns = new Dictionary<string, object>();

        public PrimitiveRow(UniqueIdentifier targetId)
        {
            _targetId = targetId;
        }

        public UniqueIdentifier TargetId
        {
            get { return _targetId; }
        }

        public string TargetName
        {
            get { return _targetId.ToString(); }//This is what the WebUI does
        }

        public object this[string key]
        {
            get { return _columns.ContainsKey(key) ? _columns[key] : null; }
        }

        internal void Update(Dictionary<string,object> newColumnValues)
        {
            _columns = newColumnValues;
            PropertyChangedEventHandler onPropertyChanged = PropertyChanged;
            if (onPropertyChanged != null)
                onPropertyChanged(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }


    }
}