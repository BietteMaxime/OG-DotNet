//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManager.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;

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
            return CreateFromViewDefinition(_remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(name), ct);
        }
        public MarketDataSnapshotProcessor CreateFromViewDefinition(ViewDefinition definition, CancellationToken ct = default(CancellationToken))
        {
            return MarketDataSnapshotProcessor.Create(_remoteEngineContext, definition, ct);
        }
    }
}
