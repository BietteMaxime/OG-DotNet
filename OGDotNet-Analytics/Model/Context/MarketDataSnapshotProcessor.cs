//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// <para>This class handles creating and mutating snapshots based on Views and live data</para>
    /// <list type="table">
    /// <item>TODO: this implementation probably shouldn't be client side</item>
    /// <item>TODO: we fetch way more data then I think is neccesary</item>
    /// </list>
    /// </summary>
    public class MarketDataSnapshotProcessor : DisposableBase
    {
        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RawMarketDataSnapper _rawMarketDataSnapper;
        private readonly RemoteMarketDataSnapshotMaster _marketDataSnapshotMaster; //TODO should be the user master

        internal static MarketDataSnapshotProcessor Create(RemoteEngineContext context, ViewDefinition definition, DateTimeOffset valuationTime, CancellationToken ct)
        {
            var rawMarketDataSnapper = new RawMarketDataSnapper(context, definition);
            var snapshot = rawMarketDataSnapper.CreateSnapshotFromView(valuationTime, ct);
            return new MarketDataSnapshotProcessor(snapshot, rawMarketDataSnapper, context.MarketDataSnapshotMaster);
        }

        internal MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, ManageableMarketDataSnapshot snapshot) 
            : this(snapshot, new RawMarketDataSnapper(remoteEngineContext, remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(snapshot.BasisViewName)), remoteEngineContext.MarketDataSnapshotMaster)
        {
        }

        private MarketDataSnapshotProcessor(ManageableMarketDataSnapshot snapshot, RawMarketDataSnapper rawMarketDataSnapper, RemoteMarketDataSnapshotMaster marketDataSnapshotMaster)
        {
            _snapshot = snapshot;
            _rawMarketDataSnapper = rawMarketDataSnapper;
            _marketDataSnapshotMaster = marketDataSnapshotMaster;
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get
            {
                CheckDisposed(); 
                return _snapshot;
            }
        }

        public UpdateAction PrepareUpdate(CancellationToken ct = default(CancellationToken))
        {
            return PrepareUpdate(s => s, ct);
        }

        public UpdateAction PrepareGlobalValuesUpdate(CancellationToken ct = default(CancellationToken))
        {
            return PrepareUpdate(s => s.GlobalValues, ct);
        }

        public UpdateAction PrepareYieldCurveUpdate(YieldCurveKey yieldCurveKey, CancellationToken ct = default(CancellationToken))
        {
            CheckDisposed();

            if (!_snapshot.YieldCurves.ContainsKey(yieldCurveKey))
            {//TODO should I be able to add a yield curve like this?
                throw new ArgumentException(string.Format("Nonexistant yield curve {0}", yieldCurveKey));
            }

            ManageableMarketDataSnapshot newSnapshot = GetNewSnapshotForUpdate(ct);
            if (newSnapshot.YieldCurves.ContainsKey(yieldCurveKey))
            {
                return PrepareUpdate(s => s.YieldCurves[yieldCurveKey], newSnapshot);
            }
            else
            {
                return _snapshot.PrepareRemoveAction(yieldCurveKey, _snapshot.YieldCurves[yieldCurveKey]);
            }
        }

        private UpdateAction PrepareUpdate<T>(Func<ManageableMarketDataSnapshot, T> scopeSelector, CancellationToken ct = default(CancellationToken)) where T : IUpdatableFrom<T>
        {
            CheckDisposed();
            ManageableMarketDataSnapshot newSnapshot = GetNewSnapshotForUpdate(ct);
            return PrepareUpdate(scopeSelector, newSnapshot);
        }

        private UpdateAction PrepareUpdate<T>(Func<ManageableMarketDataSnapshot, T> scopeSelector, ManageableMarketDataSnapshot newSnapshot) where T : IUpdatableFrom<T>
        {
            return scopeSelector(Snapshot).PrepareUpdateFrom(scopeSelector(newSnapshot));
        }

        private ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct)
        {
            return _rawMarketDataSnapper.CreateSnapshotFromView(DateTimeOffset.Now, ct);
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(CancellationToken ct = default(CancellationToken))
        {
            CheckDisposed();
            var manageableMarketDataSnapshot = _snapshot.Clone();
            manageableMarketDataSnapshot.Name = typeof(MarketDataSnapshotProcessor).Name + " Temp";
            manageableMarketDataSnapshot.UniqueId = null;
            var snapshot = _marketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, manageableMarketDataSnapshot));
            try
            {
                return _rawMarketDataSnapper.GetYieldCurves(snapshot.UniqueId, ct);
            }
            finally
            {
                _marketDataSnapshotMaster.Remove(snapshot.UniqueId);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rawMarketDataSnapper.Dispose();
            }
        }
    }
}
