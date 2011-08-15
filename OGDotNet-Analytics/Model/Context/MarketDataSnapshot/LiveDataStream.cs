//-----------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Threading;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Engine.MarketData.Spec;
using OGDotNet.Mappedtypes.Engine.View.Execution;
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
                                   RemoteEngineContext.MarketDataSnapshotter.CreateSnapshot(RemoteViewClient, cycle) );
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            //Still want to tick on time changed because it may need a recompile
            //Should probably deal with this ourselves
            var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, null, new ViewCycleExecutionOptions(default(DateTimeOffset), new LiveMarketDataSpecification()));
            remoteViewClient.AttachToViewProcess(_basisViewName, options);
        }
    }
}
