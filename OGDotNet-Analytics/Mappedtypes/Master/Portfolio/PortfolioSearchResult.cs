//-----------------------------------------------------------------------
// <copyright file="PortfolioSearchResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class PortfolioSearchResult : SearchResult<PortfolioDocument>
    {
        public PortfolioSearchResult(Paging paging, IList<PortfolioDocument> documents) : base(paging, documents)
        {
        }
    }
}