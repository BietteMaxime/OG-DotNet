//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManager.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using OGDotNet.Mappedtypes.engine.view;
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

        public MarketDataSnapshotProcessor CreateFromViewDefinition(string name, CancellationToken ct = default(CancellationToken))
        {
            return CreateFromViewDefinition(name, DateTimeOffset.Now, ct);
        }
        public MarketDataSnapshotProcessor CreateFromViewDefinition(ViewDefinition definition, CancellationToken ct = default(CancellationToken))
        {
            return CreateFromViewDefinition(definition, DateTimeOffset.Now, ct);
        }

        public MarketDataSnapshotProcessor CreateFromViewDefinition(string name, DateTimeOffset offset, CancellationToken ct = default(CancellationToken))
        {
            return CreateFromViewDefinition(_remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(name), offset, ct);
        }
        public MarketDataSnapshotProcessor CreateFromViewDefinition(ViewDefinition definition, DateTimeOffset offset, CancellationToken ct = default(CancellationToken))
        {
            return MarketDataSnapshotProcessor.Create(_remoteEngineContext, definition, offset, ct);
        }
    }
}
