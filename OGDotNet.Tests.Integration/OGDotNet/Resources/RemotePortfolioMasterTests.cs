//-----------------------------------------------------------------------
// <copyright file="RemotePortfolioMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Linq;
using OGDotNet.Mappedtypes.Master.Portfolio;
using OGDotNet.Mappedtypes.Util;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemotePortfolioMasterTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetAll()
        {
            var result = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.All, "*"));
            Assert.NotEmpty(result.Documents);
            foreach (var portfolioDocument in result.Documents)
            {
                Assert.NotNull(portfolioDocument.UniqueId);
                Assert.Equal(portfolioDocument.UniqueId, portfolioDocument.Portfolio.UniqueId);
                Assert.NotEmpty(portfolioDocument.Portfolio.Name);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetHistory()
        {
            var result = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.First(10), "*"));
            foreach (var portfolioDocument in result.Documents)
            {
                PortfolioHistoryResult portfolioHistoryResult = Context.PortfolioMaster.GetHistory(new PortfolioHistoryRequest(portfolioDocument.UniqueId.ObjectID));
                Assert.NotEmpty(portfolioHistoryResult.Documents);
                foreach (var doc in portfolioHistoryResult.Documents)
                {
                    Assert.Equal(doc.UniqueId.ObjectID, portfolioDocument.UniqueId.ObjectID);
                }
                Assert.True(portfolioHistoryResult.Documents.Any(d => d.UniqueId.Equals(portfolioDocument.UniqueId)));
            }
        }
    }
}
