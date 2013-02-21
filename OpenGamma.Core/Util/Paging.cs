// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Paging.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Util
{
    [FudgeSurrogate(typeof(PagingBuilder))]
    public class Paging
    {
        private readonly PagingRequest _request;
        private readonly int _totalItems;

        public Paging(PagingRequest request, int totalItems)
        {
            _request = request;
            _totalItems = totalItems;
        }

        public PagingRequest Request
        {
            get { return _request; }
        }

        public int TotalItems
        {
            get { return _totalItems; }
        }

        public int Pages
        {
            get
            {
                return (TotalItems - 1) / Request.Size + 1;
            }
        }

        public int CurrentPage
        {
            get
            {
                return (Request.Index / Request.Size) + 1;
            }
        }
    }
}