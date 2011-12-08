//-----------------------------------------------------------------------
// <copyright file="RemoteViewDefinitionRepositoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
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
            var repo = GetRepository();
            Assert.NotNull(repo);
        }

        protected RemoteViewDefinitionRepository GetRepository()
        {
            return Context.ViewProcessor.ViewDefinitionRepository;
        }

        [Xunit.Extensions.Fact]
        public void CanGetNames()
        {
            var viewNames = GetRepository().GetDefinitionNames();
            Assert.NotEmpty(viewNames);
            Assert.DoesNotContain(null, viewNames);
            Assert.DoesNotContain(string.Empty, viewNames);
            Assert.Contains("Equity Option Test View 1", viewNames);
        }

        [Xunit.Extensions.Fact]
        public void CanGetIds()
        {
            var ids = GetRepository().GetDefinitionIDs();
            Assert.NotEmpty(ids);
            Assert.DoesNotContain(null, ids);
            Assert.NotNull(GetRepository().GetViewDefinition(UniqueId.Create(ids.First())));
            Assert.Equal(ids.Count(), ids.Distinct().Count());
        }

        [Xunit.Extensions.Fact]
        public void CanGetViewById()
        {
            var views = GetRepository().GetDefinitionEntries();
            foreach (var view in views)
            {
                var viewDefinition = GetRepository().GetViewDefinition(view.Key);
                Assert.NotNull(viewDefinition);
                Assert.Equal(view.Value, viewDefinition.Name);
            }
        }

        [Theory]
        [TypedPropertyData("DefinitionNames")]
        public void CanGetViewDefinition(string viewName)
        {
            var remoteViewResource = GetRepository().GetViewDefinition(viewName);
            Assert.NotNull(remoteViewResource);
            Assert.Equal(viewName, remoteViewResource.Name);
            Assert.NotNull(remoteViewResource.UniqueID);
        }
    }
}