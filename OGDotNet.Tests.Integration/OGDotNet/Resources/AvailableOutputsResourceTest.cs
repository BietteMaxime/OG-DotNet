//-----------------------------------------------------------------------
// <copyright file="AvailableOutputsResourceTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Helper;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class AvailableOutputsResourceTest : ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetOutputs(ViewDefinition defn)
        {
            UniqueId portfolio = defn.PortfolioIdentifier;
            if (portfolio == null)
            {
                return;
            }
            Context.RemoteAvailableOutputs.MaxNodes = 100;
            Context.RemoteAvailableOutputs.MaxPositions = 100;
            var availableOutputs = Context.RemoteAvailableOutputs.GetPortfolioOutputs(portfolio);
            Assert.NotNull(availableOutputs);
            foreach (var securityType in availableOutputs.SecurityTypes)
            {
                ICollection<AvailableOutput> positionOutputs = availableOutputs.GetPositionOutputs(securityType);
                Assert.NotEmpty(positionOutputs);
                foreach (var availableOutput in positionOutputs)
                {
                    Assert.Null(availableOutput.PortfolioNodeProperties);
                    Assert.True(new[] {securityType}.SequenceEqual(availableOutput.PositionProperties.Keys));
                    ValueProperties positionProperty = availableOutput.PositionProperties[securityType];
                    Assert.NotNull(positionProperty);
                    Assert.NotNull(positionProperty.GetValues("Function"));
                }
            }

            ICollection<AvailableOutput> portfolioNodeOutputs = availableOutputs.GetPortfolioNodeOutputs();
            Assert.NotNull(portfolioNodeOutputs);
            foreach (var availableOutput in portfolioNodeOutputs)
            {
                Assert.Empty(availableOutput.PositionProperties);
                ValueProperties properties = availableOutput.PortfolioNodeProperties;
                Assert.NotNull(properties);
                Assert.NotNull(properties.GetValues("Function"));
            }
        }
    }
}
