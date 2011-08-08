//-----------------------------------------------------------------------
// <copyright file="SecuritySearchRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Mappedtypes.Master.Security
{
    public class SecuritySearchRequest
    {
        private readonly PagingRequest _pagingRequest;
        private readonly string _name;
        private readonly string _securityType;
        private readonly ExternalIdSearch _externalIdSearch;
        //TODO private List<ObjectId> _securityIds

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string securityType) : this(pagingRequest, name, securityType, null)
        {
        }

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string securityType, ExternalIdSearch externalIdSearch)
        {
            _pagingRequest = pagingRequest;
            _externalIdSearch = externalIdSearch;
            _name = name;
            _securityType = securityType;
        }

        public PagingRequest PagingRequest
        {
            get { return _pagingRequest; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string SecurityType
        {
            get { return _securityType; }
        }

        public ExternalIdSearch ExternalIdSearch
        {
            get { return _externalIdSearch; }
        }
    }
}