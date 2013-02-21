// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePortfolioMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Financial;
using OpenGamma.Master.Portfolio;

namespace OpenGamma.Model.Resources
{
    public class RemotePortfolioMaster : RemoteMaster<PortfolioDocument, PortfolioSearchRequest, PortfolioHistoryRequest>
    {
        public RemotePortfolioMaster(RestTarget restTarget)
            : base(restTarget, "portfolios", "portfolioSearches")
        {
        }
    }
}
