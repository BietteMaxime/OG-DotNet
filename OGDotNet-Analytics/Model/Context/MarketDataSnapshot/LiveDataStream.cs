//-----------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Threading;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class LiveDataStream : LastResultViewClient
    {
        private readonly string _basisViewName;

        public LiveDataStream(string basisViewName, RemoteEngineContext remoteEngineContext) : base(remoteEngineContext)
        {
            _basisViewName = basisViewName;
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct = default(CancellationToken))
        {
            return WithLastResults(ct,
                (cycle, graphs, results) =>
                    RawMarketDataSnapper.CreateSnapshotFromCycle(results, graphs, cycle, _basisViewName, RemoteEngineContext));
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            remoteViewClient.AttachToViewProcess(_basisViewName, ExecutionOptions.RealTime);
        }

        protected override bool ShouldWaitForExtraCycle
        {
            get { return true; }
        }
    }
}
