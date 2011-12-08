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
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Calc;
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
        private readonly Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> _specs;

        public SnapshotDataStream(ViewDefinition viewDefinition, RemoteEngineContext remoteEngineContext, UniqueId snapshotId, Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> specs)
            : base(remoteEngineContext)
        {
            ArgumentChecker.NotNull(viewDefinition, "viewDefinition");
            ArgumentChecker.NotNull(snapshotId, "snapshotId");
            _viewDefinition = viewDefinition;
            _snapshotId = snapshotId;
            _specs = specs;
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            remoteViewClient.AttachToViewProcess(_viewDefinition.UniqueID, ExecutionOptions.Snapshot(_snapshotId));
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(DateTimeOffset waitFor, CancellationToken ct)
        {
            return WithLastResults((cycle, results) => Matches(results, waitFor), ct, GetYieldCurves);
        }

        private Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(IViewCycle cycle, IViewComputationResultModel results)
        {
            var ret = new Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>>();
            foreach (var kvp in _specs)
            {
                YieldCurveKey key = kvp.Key;
                Dictionary<string, ValueRequirement> specs = kvp.Value;

                var curve = Get<YieldCurve>(specs[ValueRequirementNames.YieldCurve], results);
                var spec = Get<InterpolatedYieldCurveSpecificationWithSecurities>(specs[ValueRequirementNames.YieldCurveSpec], results);
                var interpolated = Get<NodalDoublesCurve>(specs[ValueRequirementNames.YieldCurveInterpolated], results);

                if (curve == null || spec == null || interpolated == null)
                {
                    ret.Add(key, null);
                }
                else
                {
                    ret.Add(key, new Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>(curve, spec, interpolated));
                }
            }
            return ret;
        }

        private static T Get<T>(ValueRequirement valueRequirement, IViewComputationResultModel results)
        {
            ComputedValue value;
            if (results.TryGetComputedValue("Default", valueRequirement, out value))
            {
                return (T) value.Value;
            }
            else
            {
                return default(T);
            }
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