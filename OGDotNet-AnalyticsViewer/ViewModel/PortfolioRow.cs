using System;
using System.Collections.Generic;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PortfolioRow //TODO INotifyPropertyChanged
    {
        private readonly string _positionName;
        private readonly Dictionary<string, object> _columns;

        public PortfolioRow(string positionName, Dictionary<string, object> columns)
        {
            _positionName = positionName;
            _columns = columns;
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

        public object this[String key]
        {
            get { return _columns[key]; }
        }
    }
}