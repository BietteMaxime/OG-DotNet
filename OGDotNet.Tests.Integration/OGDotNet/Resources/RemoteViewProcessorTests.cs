using System.Collections.Generic;
using System.Linq;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

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

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetViewsDefinitions(RemoteView remoteView)
        {
            var viewDefinition = remoteView.Definition;
            Assert.NotNull(viewDefinition);
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetViewsPortfoliosAfterInit(RemoteView remoteView)//NOTE: I can't test the reverse, since the view might have been inited elsewhere
        {
            remoteView.Init();
            var portfolio = remoteView.Portfolio;

            if (remoteView.Name != "Primitives Only")
            {
                Assert.NotNull(portfolio);
            }
            else
            {
                Assert.Null(portfolio);
            }
        }
    }
}
