//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManager.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// A factory for <see cref="MarketDataSnapshotProcessor"/>s
    /// </summary>
    public class MarketDataSnapshotManager
    {
        private readonly RemoteEngineContext _remoteEngineContext;
        
        public MarketDataSnapshotManager(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
        }


        public MarketDataSnapshotProcessor GetProcessor(ManageableMarketDataSnapshot snapshot)
        {
            return new MarketDataSnapshotProcessor(_remoteEngineContext, snapshot);    
        }


        public MarketDataSnapshotProcessor CreateFromView(string viewName, CancellationToken ct =default(CancellationToken))
        {
            return CreateFromView(viewName,DateTimeOffset.Now, ct);
        }
        public MarketDataSnapshotProcessor CreateFromView(RemoteView view, CancellationToken ct = default(CancellationToken))
        {
            return CreateFromView(view,DateTimeOffset.Now,ct);
        }

        public MarketDataSnapshotProcessor CreateFromView(string viewName, DateTimeOffset offset, CancellationToken ct = default(CancellationToken))
        {
            return CreateFromView(_remoteEngineContext.ViewProcessor.GetView(viewName), offset,ct);
        }
        public MarketDataSnapshotProcessor CreateFromView(RemoteView view, DateTimeOffset offset, CancellationToken ct = default(CancellationToken))
        {
            return MarketDataSnapshotProcessor.Create(_remoteEngineContext, view, offset, ct);
        }
    }
}
