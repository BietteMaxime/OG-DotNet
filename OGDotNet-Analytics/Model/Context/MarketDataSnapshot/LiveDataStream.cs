//-----------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Threading;
using OGDotNet.Mappedtypes.engine.marketdata.spec;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class LiveDataStream : LastResultViewClient
    {
        public event EventHandler BasisViewNameChanged;

        private string _basisViewName;

        public LiveDataStream(string basisViewName, RemoteEngineContext remoteEngineContext) : base(remoteEngineContext)
        {
            _basisViewName = basisViewName;
        }

        public string BasisViewName
        {
            get
            {
                return _basisViewName;
            }
            set
            {
                _basisViewName = value;
                Reattach(); //TODO should be on another thread
                InvokeBasisViewNameChanged(EventArgs.Empty);
            }
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct = default(CancellationToken))
        {
            return WithLastResults(ct,
                (cycle, graphs, results) =>
                    RawMarketDataSnapper.CreateSnapshotFromCycle(results, graphs, cycle, _basisViewName, RemoteEngineContext));
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            //Still want to tick on time changed because it may need a recompile
            //Should probably deal with this ourselves
            var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, null, new ViewCycleExecutionOptions(default(DateTimeOffset), new LiveMarketDataSpecification()));
            remoteViewClient.AttachToViewProcess(_basisViewName, options);
        }

        public void InvokeBasisViewNameChanged(EventArgs e)
        {
            EventHandler handler = BasisViewNameChanged;
            if (handler != null) handler(this, e);
        }
    }
}
