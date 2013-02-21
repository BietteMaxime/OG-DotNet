// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Financial.view.rest;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class RemoteViewProcessorTests : ViewTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            Assert.NotNull(remoteViewProcessor);
        }

        [Xunit.Extensions.Fact]
        public void CanGetNamedMarketDataSpecifications()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            RemoteNamedMarketDataSpecificationRepository remoteNamedMarketDataSpecificationRepository = remoteViewProcessor.LiveMarketDataSourceRegistry;
            IEnumerable<string> specificationNames = remoteNamedMarketDataSpecificationRepository.GetNames();
            Assert.NotEmpty(specificationNames);
            Assert.Contains("Live market data (TullettPrebon, Bloomberg, Activ, ICAP)", specificationNames);
        }
    }
}
