// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteSecurityMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Db;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteSecurityMasterTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanSearchWithRequest()
        {
            var searchResult = Context.SecurityMaster.Search("*", "FUTURE", PagingRequest.All);
            Assert.NotEmpty(searchResult.Documents);

            var securitytoFind = searchResult.Documents.First();
            var identifierBundle = securitytoFind.Security.Identifiers;
            {
                var identifierSearch = new IdentifierSearch(identifierBundle.Identifiers, IdentifierSearchType.All);
                var singleSearchResult = Context.SecurityMaster.Search("*", "FUTURE", PagingRequest.All, identifierSearch);
                Assert.NotEmpty(singleSearchResult.Documents);
                Assert.Single(singleSearchResult.Documents);
                Assert.Equal(singleSearchResult.Documents.Single().Security.UniqueId, UniqueIdentifier.Parse(securitytoFind.UniqueId));
            }
            {
                var identifierSearch = new IdentifierSearch(identifierBundle.Identifiers.Concat(Enumerable.Repeat(Identifier.Of("XXX", "YYY"), 1)), IdentifierSearchType.Any);
                var singleSearchResult = Context.SecurityMaster.Search("*", "FUTURE", PagingRequest.All, identifierSearch);
                Assert.NotEmpty(singleSearchResult.Documents);
                Assert.Single(singleSearchResult.Documents);
                Assert.Equal(singleSearchResult.Documents.Single().Security.UniqueId, UniqueIdentifier.Parse(securitytoFind.UniqueId));
            }
        }
    }
}