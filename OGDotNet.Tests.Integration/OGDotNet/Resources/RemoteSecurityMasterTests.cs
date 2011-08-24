// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteSecurityMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.Security;
using OGDotNet.Mappedtypes.Util;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteSecurityMasterTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanSearchWithRequest()
        {
            var request = new SecuritySearchRequest(PagingRequest.First(20), "*", "FUTURE");
            var searchResult = Context.SecurityMaster.Search(request);
            Assert.NotEmpty(searchResult.Documents);

            var securitytoFind = searchResult.Documents.First();
            var identifierBundle = securitytoFind.Security.Identifiers;
            {
                var identifierSearch = new ExternalIdSearch(identifierBundle.Identifiers, IdentifierSearchType.All);
                request = new SecuritySearchRequest(PagingRequest.All, "*", "FUTURE", identifierSearch);
                var singleSearchResult = Context.SecurityMaster.Search(request);
                Assert.NotEmpty(singleSearchResult.Documents);
                Assert.Single(singleSearchResult.Documents);
                Assert.Equal(singleSearchResult.Documents.Single().Security.UniqueId, securitytoFind.UniqueId);
            }
            {
                var identifierSearch = new ExternalIdSearch(identifierBundle.Identifiers.Concat(Enumerable.Repeat(ExternalId.Of("XXX", "YYY"), 1)), IdentifierSearchType.Any);
                request = new SecuritySearchRequest(PagingRequest.All, "*", "FUTURE", identifierSearch);
                var singleSearchResult = Context.SecurityMaster.Search(request);
                Assert.NotEmpty(singleSearchResult.Documents);
                Assert.Single(singleSearchResult.Documents);
                Assert.Equal(singleSearchResult.Documents.Single().Security.UniqueId, securitytoFind.UniqueId);
            }
        }

        [Xunit.Extensions.Fact]
        public void GetsMatchSearch()
        {
            var request = new SecuritySearchRequest(PagingRequest.One, "*", "FUTURE", null);
            var searchResult = Context.SecurityMaster.Search(request);
            foreach (var securityDocument in searchResult.Documents)
            {
                var security = Context.SecurityMaster.GetSecurity(securityDocument.UniqueId);
                Assert.NotNull(security);
                Assert.Equal(securityDocument.Security.Name, security.Name);
            }
        }

        [Xunit.Extensions.Fact]
        public void PagingBehavesAsExpected()
        {
            const int initialDocs = 10;
            var request = new SecuritySearchRequest(PagingRequest.First(initialDocs), "*", "FUTURE", null);
            var intialResult = Context.SecurityMaster.Search(request);
            Assert.Equal(initialDocs, intialResult.Documents.Count);
            Assert.InRange(intialResult.Paging.TotalItems, initialDocs, int.MaxValue);

            request = new SecuritySearchRequest(PagingRequest.OfIndex(0, 1), "*", "FUTURE", null);
            var searchResult = Context.SecurityMaster.Search(request);
            Assert.Equal(1, searchResult.Documents.Count);
            Assert.Equal(intialResult.Documents[0].UniqueId, searchResult.Documents.Single().UniqueId);

            request = new SecuritySearchRequest(PagingRequest.OfPage(1, 1), "*", "FUTURE", null);
            searchResult = Context.SecurityMaster.Search(request);
            Assert.Equal(1, searchResult.Documents.Count);
            Assert.Equal(intialResult.Documents[0].UniqueId, searchResult.Documents.Single().UniqueId);

            request = new SecuritySearchRequest(PagingRequest.OfIndex(2, 1), "*", "FUTURE", null);
            searchResult = Context.SecurityMaster.Search(request);
            Assert.Equal(1, searchResult.Documents.Count);
            Assert.Equal(intialResult.Documents[2].UniqueId, searchResult.Documents.Single().UniqueId);

            request = new SecuritySearchRequest(PagingRequest.OfIndex(intialResult.Paging.TotalItems, 1), "*", "FUTURE", null);
            searchResult = Context.SecurityMaster.Search(request);
            Assert.Empty(searchResult.Documents);
        }

        [Xunit.Extensions.Fact]
        public void CanQueryMetadata()
        {
            var result = Context.SecurityMaster.MetaData(new SecurityMetaDataRequest());
            Assert.NotNull(result);
            Assert.NotEmpty(result.SecurityTypes);
            Assert.Contains("FUTURE", result.SecurityTypes);
            foreach (var securityType in result.SecurityTypes)
            {
                var request = new SecuritySearchRequest(PagingRequest.One, "*", securityType, null);
                var searchResult = Context.SecurityMaster.Search(request);
                switch (securityType)
                {
                    default:
                        Assert.NotEqual(0, searchResult.Paging.TotalItems);
                        Assert.Equal(securityType, searchResult.Documents.Single().Security.SecurityType);
                        break;
                }
            }
            var request1 = new SecuritySearchRequest(PagingRequest.None, "*", null, null);
            Assert.Equal(
                Context.SecurityMaster.Search(request1).Paging.TotalItems,
                result.SecurityTypes.Sum(r =>
                                             {
                                                 var request = new SecuritySearchRequest(PagingRequest.None, "*", r, null);
                                                 return Context.SecurityMaster.Search(request).Paging.TotalItems;
                                             })
                );
        }
    }
}