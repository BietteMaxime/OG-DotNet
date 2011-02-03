using System;
using System.Collections.Generic;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.View
{
    public class PortfolioRow
    {
        private readonly string _positionName;
        private readonly UniqueIdentifier _id;
        private readonly Dictionary<string, object> _columns;

        public PortfolioRow(UniqueIdentifier id, string positionName, Dictionary<string, object> columns)
        {
            _id = id;
            _positionName = positionName;
            _columns = columns;
        }

        public UniqueIdentifier Id
        {
            get { return _id; }
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