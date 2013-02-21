// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Engine.Value;
using OpenGamma.Engine.View.Calc;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot;
using OpenGamma.MarketDataSnapshot.Impl;
using OpenGamma.Model;

namespace OpenGamma.Financial.view.rest
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
