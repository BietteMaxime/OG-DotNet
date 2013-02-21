// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SnapshotDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

using OpenGamma.Analytics.Financial.Model.InterestRate.Curve;
using OpenGamma.Analytics.Math.Curve;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Calc;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Financial.Analytics.IRCurve;
using OpenGamma.Financial.view.rest;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot;
using OpenGamma.Model.Resources;
using OpenGamma.Util;

namespace OpenGamma.Model.Context.MarketDataSnapshot
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
            remoteViewClient.AttachToViewProcess(_viewDefinition.UniqueId, ExecutionOptions.Snapshot(_snapshotId));
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