//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotSearchResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotSearchResult : SearchResult<MarketDataSnapshotDocument>
    {
        public MarketDataSnapshotSearchResult(Paging paging, IList<MarketDataSnapshotDocument> documents) : base(paging, documents)
        {
        }
    }
}
