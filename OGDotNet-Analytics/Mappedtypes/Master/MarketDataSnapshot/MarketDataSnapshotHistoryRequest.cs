//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotHistoryRequest : AbstractHistoryRequest
    {
        private readonly bool _includeData;

        public MarketDataSnapshotHistoryRequest(ObjectId objectId, bool includeData) : base(objectId)
        {
            _includeData = includeData;
        }

        public bool IncludeData
        {
            get { return _includeData; }
        }
    }
}
