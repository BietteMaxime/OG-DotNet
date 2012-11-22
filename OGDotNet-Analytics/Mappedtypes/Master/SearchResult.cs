//-----------------------------------------------------------------------
// <copyright file="SearchResult.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master
{
    public class SearchResult<TDocument> where TDocument : AbstractDocument
    {
        private readonly Paging _paging;
        private readonly VersionCorrection _versionCorrection;
        private readonly IList<TDocument> _documents;

        public SearchResult(Paging paging, VersionCorrection versionCorrection, IList<TDocument> documents)
        {
            _paging = paging;
            _versionCorrection = versionCorrection;
            _documents = documents;
        }

        public Paging Paging
        {
            get { return _paging; }
        }

        public VersionCorrection VersionCorrection
        {
            get { return _versionCorrection; }
        }

        public IList<TDocument> Documents
        {
            get { return _documents; }
        }
    }
}