//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public class RemoteMarketDataSnapshotter
    {
        private readonly RestTarget _rest;

        public RemoteMarketDataSnapshotter(RestTarget rest)
        {
            _rest = rest;
        }

        public ManageableMarketDataSnapshot CreateSnapshot(RemoteViewClient client, IViewCycle cycle)
        {
            UniqueId clientId = client.GetUniqueId();
            UniqueId cycleId = cycle.UniqueId;

            var createTarget = _rest.Resolve("create", clientId.ToString(), cycleId.ToString());
            return createTarget.Get<ManageableMarketDataSnapshot>();
        }
        public Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> GetYieldCurveRequirements(RemoteViewClient client, IViewCycle cycle)
        {
            UniqueId clientId = client.GetUniqueId();
            UniqueId cycleId = cycle.UniqueId;

            var createTarget = _rest.Resolve("yieldCurveSpecs", clientId.ToString(), cycleId.ToString());
            return createTarget.Get<Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>>>();
        }
    }
}
