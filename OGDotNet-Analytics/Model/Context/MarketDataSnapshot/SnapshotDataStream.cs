//-----------------------------------------------------------------------
// <copyright file="SnapshotDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStream : LastResultViewClient
    {
        private readonly string _basisViewName;
        private readonly UniqueIdentifier _snapshotId;
        private readonly LiveDataStream _liveDataStream;

        public SnapshotDataStream(string basisViewName, RemoteEngineContext remoteEngineContext, UniqueIdentifier snapshotId, LiveDataStream liveDataStream) : base(remoteEngineContext)
        {
            _basisViewName = basisViewName;
            _snapshotId = snapshotId;
            _liveDataStream = liveDataStream;
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            remoteViewClient.AttachToViewProcess(_basisViewName, ExecutionOptions.Snapshot(_snapshotId));
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities>> GetYieldCurves(DateTimeOffset waitFor, CancellationToken ct)
        {
            return WithLastResults(waitFor, ct, (cycle, graphs, results) => RawMarketDataSnapper.EvaluateYieldCurves(cycle, graphs));
        }
    }
}