//-----------------------------------------------------------------------
// <copyright file="RemoteViewProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using OGDotNet.Model.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewProcessorTests : ViewTestsBase
    {
        [Fact]
        public void CanGet()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            Assert.NotNull(remoteViewProcessor);
        }

        [Fact]
        public void CanGetMarketDataSources()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            RemoteLiveMarketDataSourceRegistry remoteLiveMarketDataSourceRegistry = remoteViewProcessor.LiveMarketDataSourceRegistry;
            IEnumerable<string> dataSources = remoteLiveMarketDataSourceRegistry.GetDataSources();
            Assert.NotEmpty(dataSources);
            Assert.Contains(null, dataSources);
            Assert.Contains("Combined", dataSources);
        }
    }
}
