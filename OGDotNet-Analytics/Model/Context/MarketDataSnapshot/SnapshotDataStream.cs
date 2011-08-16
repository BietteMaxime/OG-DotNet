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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Math.Curve;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStream : LastResultViewClient
    {
        private readonly ViewDefinition _viewDefinition;
        private readonly UniqueId _snapshotId;

        public SnapshotDataStream(ViewDefinition viewDefinition, RemoteEngineContext remoteEngineContext, UniqueId snapshotId)
            : base(remoteEngineContext)
        {
            ArgumentChecker.NotNull(viewDefinition, "viewDefinition");
            ArgumentChecker.NotNull(snapshotId, "snapshotId");
            _viewDefinition = viewDefinition;
            _snapshotId = snapshotId;
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            remoteViewClient.AttachToViewProcess(_viewDefinition.Name, ExecutionOptions.Snapshot(_snapshotId));
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(DateTimeOffset waitFor, CancellationToken ct)
        {
            return WithLastResults((cycle, results) => Matches(results, waitFor),
                ct, (cycle, results) => RawMarketDataSnapper.EvaluateYieldCurves(results, _viewDefinition));
        }

        private static bool Matches(IViewComputationResultModel results, DateTimeOffset waitFor)
        {
            if (results.ResultTimestamp < waitFor)
            {
                return false;
            }            
            
            return true;
        }
    }
}