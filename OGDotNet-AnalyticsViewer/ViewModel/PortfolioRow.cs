using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Security;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PortfolioRow //TODO INotifyPropertyChanged
    {
        private readonly string _positionName;
        private readonly Dictionary<string, object> _columns;
        private readonly Security _security;

        public PortfolioRow(string positionName, Dictionary<string, object> columns, Security security)
        {
            _positionName = positionName;
            _columns = columns;
            _security = security;
        }

        public string PositionName
        {
            get
            {
                return _positionName;
            }
        }

        public Dictionary<string, object> Columns
        {
            get
            {
                return _columns;
            }
        }

        public Security Security
        {
            get { return _security; }
        }

        public object this[String key]
        {
            get { return _columns[key]; }
        }
    }
}