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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Math.Curve;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// <para>This class handles creating and mutating snapshots based on Views and live data</para>
    /// <list type="table">
    /// <item>TODO: this implementation probably shouldn't be client side</item>
    /// </list>
    /// </summary>
    public class MarketDataSnapshotProcessor : DisposableBase
    {
        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RemoteMarketDataSnapshotMaster _marketDataSnapshotMaster;
        private readonly SnapshotLiveDataStreamInvalidater _liveDataStream;
        private readonly SnapshotDataStreamInvalidater _snapshotDataStream;

        private readonly object _snapshotUidLock = new object();
        private readonly RemoteClient _remoteClient;
        private UniqueIdentifier _temporarySnapshotUid;

        internal static MarketDataSnapshotProcessor Create(RemoteEngineContext context, ViewDefinition definition, CancellationToken ct)
        {
            using (var liveDataStream = new LiveDataStream(definition.Name, context))
            {
                ManageableMarketDataSnapshot snapshot = liveDataStream.GetNewSnapshotForUpdate(ct);
                //NOTE: we could consider reusing the LiveDataStream, but server side will share the processer
                return new MarketDataSnapshotProcessor(context, snapshot);
            }
        }

        internal MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, ManageableMarketDataSnapshot snapshot)
            : this(snapshot, remoteEngineContext, new SnapshotLiveDataStreamInvalidater(snapshot, remoteEngineContext))
        {
        }

        private MarketDataSnapshotProcessor(ManageableMarketDataSnapshot snapshot, RemoteEngineContext remoteEngineContext, SnapshotLiveDataStreamInvalidater liveDataStream)
        {
            _snapshot = snapshot;
            _remoteClient = remoteEngineContext.CreateUserClient();
            _marketDataSnapshotMaster = _remoteClient.MarketDataSnapshotMaster;
            _liveDataStream = liveDataStream;
            _temporarySnapshotUid = _marketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, GetShallowCloneSnapshot())).UniqueId;
            _snapshotDataStream = new SnapshotDataStreamInvalidater(_liveDataStream, remoteEngineContext, _temporarySnapshotUid);
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get
            {
                CheckDisposed(); 
                return _snapshot;
            }
        }

        public UpdateAction<ManageableMarketDataSnapshot> PrepareUpdate(CancellationToken ct = default(CancellationToken))
        {
            return Snapshot.PrepareUpdateFrom(GetNewSnapshotForUpdate(ct));
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct = default(CancellationToken))
        {
            return _liveDataStream.With(ct, l => l.GetNewSnapshotForUpdate(ct));
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(CancellationToken ct = default(CancellationToken))
        {
            CheckDisposed();

            DateTimeOffset waitFor;
            ManageableMarketDataSnapshot shallowClone = GetShallowCloneSnapshot();
            lock (_snapshotUidLock)
            {
                shallowClone.UniqueId = _temporarySnapshotUid;
                var snapshot = _marketDataSnapshotMaster.Update(new MarketDataSnapshotDocument(_temporarySnapshotUid, shallowClone));
                _temporarySnapshotUid = snapshot.UniqueId;

                waitFor = _marketDataSnapshotMaster.Get(_temporarySnapshotUid).CorrectionFromInstant;
            }
            return _snapshotDataStream.With(ct, s => s.GetYieldCurves(waitFor, ct));
        }

        private ManageableMarketDataSnapshot GetShallowCloneSnapshot()
        {
            return new ManageableMarketDataSnapshot(_snapshot.BasisViewName, _snapshot.GlobalValues, _snapshot.YieldCurves, _snapshot.VolatilityCubes, _snapshot.VolatilitySurfaces, _snapshot.UniqueId)
                       {
                           Name = string.Format("{0}-{1}-{2}", typeof(MarketDataSnapshotProcessor).Name, Guid.NewGuid(), _snapshot.BasisViewName),
                           UniqueId = null
                       };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _snapshotDataStream.Dispose();
                _liveDataStream.Dispose();
                _marketDataSnapshotMaster.Remove(_temporarySnapshotUid.ToLatest());
                _remoteClient.Dispose();
            }
        }
    }
}
