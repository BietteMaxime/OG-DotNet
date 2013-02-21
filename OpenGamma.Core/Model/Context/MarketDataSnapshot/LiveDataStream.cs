// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Financial.view.rest;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot.Impl;

namespace OpenGamma.Model.Context.MarketDataSnapshot
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
            return WithLastResults(ct, (cycle, results) =>
                                       RemoteEngineContext.ViewProcessor.MarketDataSnapshotter.CreateSnapshot(
                                           RemoteViewClient, cycle));
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            var viewDefinition = RemoteEngineContext.ConfigSource.Get<ViewDefinition>(_basisViewName);
            if (viewDefinition == null)
            {
                throw new OpenGammaException("No view definition found with name " + _basisViewName);
            }

            UniqueId viewDefinitionId = viewDefinition.UniqueId;

            // Still want to tick on time changed because it may need a recompile
            // Should probably deal with this ourselves
            var marketDataSpecifications = new List<MarketDataSpecification> {new LiveMarketDataSpecification()};
            var options = new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled | ViewExecutionFlags.AwaitMarketData, null, new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications));
            remoteViewClient.AttachToViewProcess(viewDefinitionId, options);
        }
    }
}
