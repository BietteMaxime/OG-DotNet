// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.View.Calc;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Financial.view.rest;

using Xunit;

namespace OpenGamma.Model.Resources
{
    /// <summary>
    /// At the moment this is largely tested via <see cref="MarketDataSnapshotManagerTests"/> and <see cref="MarketDataSnapshotProcessorTests"/> 
    /// </summary>
    public class RemoteMarketDataSnapshotterTests : ViewTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetSnapshot()
        {
            RemoteMarketDataSnapshotter snapshotter = Context.ViewProcessor.MarketDataSnapshotter;
            RemoteViewCycleTests.WithViewCycle(
                delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
                    {
                        var snapshot = snapshotter.CreateSnapshot(client, cycle);
                        Assert.NotNull(snapshot);
                    }, Fixture.EquityViewDefinition.UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanGetYieldCurveSpecs()
        {
            RemoteMarketDataSnapshotter snapshotter = Context.ViewProcessor.MarketDataSnapshotter;
            RemoteViewCycleTests.WithViewCycle(
                delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
                    {
                        var snapshot = snapshotter.GetYieldCurveRequirements(client, cycle);
                        Assert.NotNull(snapshot);
                        Assert.NotEmpty(snapshot);
                    }, Fixture.EquityViewDefinition.UniqueId);
        }
    }
}
