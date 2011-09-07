//-----------------------------------------------------------------------
// <copyright file="PortfolioSearchRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class PortfolioSearchRequest
    {
        //TODO: this
        private readonly PagingRequest _pagingRequest;
        private readonly string _name;

        public PortfolioSearchRequest(PagingRequest pagingRequest, string name)
        {
            _pagingRequest = pagingRequest;
            _name = name;
        }

        public PagingRequest PagingRequest
        {
            get { return _pagingRequest; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
