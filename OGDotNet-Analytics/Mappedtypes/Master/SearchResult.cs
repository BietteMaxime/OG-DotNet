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
        private readonly Paging _paging;
        private readonly IList<TDocument> _documents;

        public SearchResult(Paging paging, IList<TDocument> documents)
        {
            _paging = paging;
            _documents = documents;
        }

        public Paging Paging
        {
            get { return _paging; }
        }

        public IList<TDocument> Documents
        {
            get { return _documents; }
        }
    }
}