using System;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

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


        public MarketDataSnapshotProcessor CreateFromView(string viewName)
        {
            return CreateFromView(viewName,DateTimeOffset.Now);
        }
        public MarketDataSnapshotProcessor CreateFromView(RemoteView view)
        {
            return CreateFromView(view,DateTimeOffset.Now);
        }

        public MarketDataSnapshotProcessor CreateFromView(string viewName, DateTimeOffset offset)
        {
            return CreateFromView(_remoteEngineContext.ViewProcessor.GetView(viewName), offset);
        }
        public MarketDataSnapshotProcessor CreateFromView(RemoteView view, DateTimeOffset offset)
        {
            return MarketDataSnapshotProcessor.Create(_remoteEngineContext, view, offset);
        }
    }
}
