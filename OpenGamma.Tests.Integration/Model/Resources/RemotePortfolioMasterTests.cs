// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePortfolioMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using OpenGamma.Core.Change;
using OpenGamma.Id;
using OpenGamma.Master.Portfolio;
using OpenGamma.Util;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class RemotePortfolioMasterTests : ViewTestBase
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
        public void CanGetByUid()
        {
            var all = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.All, "*"));
            PortfolioDocument doc = all.Documents.First();
            var singleResult = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.All, new List<ObjectId> {doc.UniqueId.ObjectId}, null));
            Assert.Equal(1, singleResult.Documents.Count);
            Assert.Equal(doc.UniqueId, singleResult.Documents.Single().UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanGetHistory()
        {
            var result = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.First(10), "*"));
            foreach (var portfolioDocument in result.Documents)
            {
                var portfolioHistoryResult = Context.PortfolioMaster.History(new PortfolioHistoryRequest(portfolioDocument.UniqueId.ObjectId));
                Assert.NotEmpty(portfolioHistoryResult.Documents);
                foreach (var doc in portfolioHistoryResult.Documents)
                {
                    Assert.Equal(doc.UniqueId.ObjectId, portfolioDocument.UniqueId.ObjectId);
                }

                Assert.True(portfolioHistoryResult.Documents.Any(d => d.UniqueId.Equals(portfolioDocument.UniqueId)));
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetBigHistory()
        {
            var result = Context.PortfolioMaster.Search(new PortfolioSearchRequest(PagingRequest.First(10), "web*"));
            foreach (var portfolioDocument in result.Documents)
            {
                var portfolioHistoryResult = Context.PortfolioMaster.History(new PortfolioHistoryRequest(portfolioDocument.UniqueId.ObjectId, 0));
                Assert.NotEmpty(portfolioHistoryResult.Documents);
                foreach (var doc in portfolioHistoryResult.Documents)
                {
                    Assert.Equal(doc.UniqueId.ObjectId, portfolioDocument.UniqueId.ObjectId);

                    // TODO assert depth respected
                }

                Assert.True(portfolioHistoryResult.Documents.Any(d => d.UniqueId.Equals(portfolioDocument.UniqueId)));
            }
        }

        /*
        [Xunit.Extensions.Fact]
        public void CanGetChangeManager()
        {
            var remoteChangeManger = Context.PortfolioMaster.ChangeManager;
            var changeListener = new AggregatingChangeListener();
            remoteChangeManger.AddChangeListener(changeListener);
            try
            {
                Assert.Empty(changeListener.GetAndClearEvents());
            }
            finally
            {
                remoteChangeManger.RemoveChangeListener(changeListener);
            }
        }
         */
    }
}
