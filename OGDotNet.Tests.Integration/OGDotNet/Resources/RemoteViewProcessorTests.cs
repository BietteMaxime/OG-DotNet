//-----------------------------------------------------------------------
// <copyright file="RemoteViewProcessorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Tests.Integration.Xunit.Extensions;
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
        public void CanGetNames()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            var viewNames = remoteViewProcessor.ViewNames;
            Assert.NotEmpty(viewNames);
            Assert.DoesNotContain(null, viewNames);
            Assert.DoesNotContain("", viewNames);
        }

        [Theory]
        [TypedPropertyData("ViewNames")]
        public void CanGetViews(string viewName)
        {
            var remoteViewResource = Context.ViewProcessor.GetView(viewName);
            Assert.NotNull(remoteViewResource);
            Assert.Equal(viewName, remoteViewResource.Name);

            
        }
    }
}
