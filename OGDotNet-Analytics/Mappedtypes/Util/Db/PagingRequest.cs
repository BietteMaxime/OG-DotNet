//-----------------------------------------------------------------------
// <copyright file="PagingRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Util.Db
{
    [FudgeSurrogate(typeof(PagingRequestBuilder))]
    public class PagingRequest
    {
        public static readonly PagingRequest All = new PagingRequest(0, int.MaxValue);
        public static readonly PagingRequest None = new PagingRequest(0, 0);
        public static readonly PagingRequest One = new PagingRequest(0, 1);
        private readonly int _index;
        public int Index
        {
            get { return _index; }
        }

        private readonly int _size;
        public int Size
        {
            get { return _size; }
        }

        private PagingRequest(int index, int size)
        {
            _index = index;
            _size = size;
        }

        public static PagingRequest OfIndex(int index, int size)
        {
            return new PagingRequest(index, size);
        }

        public static PagingRequest OfPage(int page, int pagingSize)
        {
            int index = (page - 1) * pagingSize;
            return new PagingRequest(index, pagingSize);
        }

        public static PagingRequest First(int n)
        {
            return new PagingRequest(0, n);
        }
    }
}