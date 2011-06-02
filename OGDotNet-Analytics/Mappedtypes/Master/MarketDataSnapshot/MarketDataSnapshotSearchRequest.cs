//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotSearchRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotSearchRequest
    {
        private readonly bool _includeData;
        private readonly PagingRequest _pagingRequest;
        private readonly string _name;
        //TODO private readonly List<Identifier> _snapshotIds;

        public MarketDataSnapshotSearchRequest(string name, PagingRequest pagingRequest, bool includeData = true)
        {
            _pagingRequest = pagingRequest;
            _includeData = includeData;
            _name = name;
        }

        public PagingRequest PagingRequest
        {
            get { return _pagingRequest; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool IncludeData
        {
            get { return _includeData; }
        }
    }
}
