// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Financial;
using OpenGamma.Master.MarketDataSnapshot;

namespace OpenGamma.Model.Resources
{
    public class RemoteMarketDataSnapshotMaster : RemoteMaster<MarketDataSnapshotDocument, MarketDataSnapshotSearchRequest, MarketDataSnapshotHistoryRequest>
    {
        public RemoteMarketDataSnapshotMaster(RestTarget restTarget)
            : base(restTarget, "snapshots", "snapshotSearches")
        {
        }
    }
}
