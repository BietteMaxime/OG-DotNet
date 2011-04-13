//-----------------------------------------------------------------------
// <copyright file="PagingRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Util.Db
{
    public class PagingRequest
    {
        public static readonly PagingRequest All = new PagingRequest(1, int.MaxValue);
        private readonly int _page;
        public int Page
        {
            get { return _page; }
        }

        private readonly int _pagingSize;
        public int PagingSize
        {
            get { return _pagingSize; }
        }

        public PagingRequest(int page, int pagingSize)
        {
            _page = page;
            _pagingSize = pagingSize;
        }
    }
}