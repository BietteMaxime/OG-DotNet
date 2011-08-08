//-----------------------------------------------------------------------
// <copyright file="RemoteViewDefinitionRepositoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewDefinitionRepositoryTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            var repo = Context.ViewProcessor.ViewDefinitionRepository;
            Assert.NotNull(repo);
        }

        [Xunit.Extensions.Fact]
        public void CanGetNames()
        {
            var remoteViewProcessor = Context.ViewProcessor;
            var viewNames = remoteViewProcessor.ViewDefinitionRepository.GetDefinitionNames();
            Assert.NotEmpty(viewNames);
            Assert.DoesNotContain(null, viewNames);
            Assert.DoesNotContain(string.Empty, viewNames);
        }

        [Theory]
        [TypedPropertyData("DefinitionNames")]
        public void CanGetViewDefinitions(string viewName)
        {
            var remoteViewResource = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewName);
            Assert.NotNull(remoteViewResource);
            Assert.Equal(viewName, remoteViewResource.Name);
            Assert.NotNull(remoteViewResource.UniqueID);
        }
    }
}