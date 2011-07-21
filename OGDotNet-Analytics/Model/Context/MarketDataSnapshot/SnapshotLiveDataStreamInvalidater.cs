//-----------------------------------------------------------------------
// <copyright file="SnapshotLiveDataStreamInvalidater.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Threading;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotLiveDataStreamInvalidater : Invalidater<LiveDataStream>
    {
        private readonly ManualResetEventSlim _constructedEvent = new ManualResetEventSlim(false);
        public event EventHandler GraphChanged;

        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RemoteEngineContext _remoteEngineContext;

        public SnapshotLiveDataStreamInvalidater(ManageableMarketDataSnapshot snapshot, RemoteEngineContext remoteEngineContext)
        {
            _snapshot = snapshot;
            _remoteEngineContext = remoteEngineContext;
            snapshot.PropertyChanged += PropertyChanged; //TODO: weak ref

            _constructedEvent.Set();
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BasisViewName")
            {
                Invalidate();
                InvokeGraphChanged(EventArgs.Empty);
            }
        }

        protected override LiveDataStream Build(CancellationToken ct)
        {
            _constructedEvent.Wait(ct);

            var liveDataStream = new LiveDataStream(_snapshot.BasisViewName, _remoteEngineContext);
            liveDataStream.GraphChanged += (sender, e) => InvokeGraphChanged(e);
            return liveDataStream;
        }

        public void InvokeGraphChanged(EventArgs e)
        {
            EventHandler handler = GraphChanged;
            if (handler != null) handler(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _snapshot.PropertyChanged -= PropertyChanged;
            }
        }
    }
}