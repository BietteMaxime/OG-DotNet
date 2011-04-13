//-----------------------------------------------------------------------
// <copyright file="SearchResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Mappedtypes.Master
{
    public class SearchResult<TDocument> //where TDocument extends Document
    {
        public Paging Paging { get; set; }
        public IList<TDocument> Documents { get; set; }
    }
}