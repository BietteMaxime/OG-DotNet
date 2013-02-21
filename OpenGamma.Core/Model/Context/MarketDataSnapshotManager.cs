// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManager.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading;

using OpenGamma.Engine.View;
using OpenGamma.MarketDataSnapshot.Impl;

namespace OpenGamma.Model.Context
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

        public MarketDataSnapshotProcessor CreateFromViewDefinition(string name, CancellationToken ct = default(CancellationToken))
        {
            var viewDefinition = _remoteEngineContext.ViewProcessor.ConfigSource.Get<ViewDefinition>(name, null);
            return CreateFromViewDefinition(viewDefinition, ct);
        }

        public MarketDataSnapshotProcessor CreateFromViewDefinition(ViewDefinition definition, CancellationToken ct = default(CancellationToken))
        {
            return MarketDataSnapshotProcessor.Create(_remoteEngineContext, definition, ct);
        }
    }
}
