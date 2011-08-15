//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Model.Resources;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    /// <summary>
    /// At the moment this is largely tested via <see cref="MarketDataSnapshotManagerTests"/> and <see cref="MarketDataSnapshotProcessorTests"/> 
    /// </summary>
    public class RemoteMarketDataSnapshotterTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetSnapshot()
        {
            RemoteMarketDataSnapshotter snapshotter = Context.MarketDataSnapshotter;
            RemoteViewCycleTests.WithViewCycle(
           delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
               {
                   var snapshot = snapshotter.CreateSnapshot(client, cycle);
                   Assert.NotNull(snapshot);
               });
        }
    }
}
