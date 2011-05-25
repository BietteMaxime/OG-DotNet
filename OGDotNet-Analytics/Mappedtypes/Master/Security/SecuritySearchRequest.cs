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
    internal class SecuritySearchRequest
    {
        private readonly PagingRequest _pagingRequest;
        private readonly string _name;
        private readonly string _securityType;
        private readonly IdentifierSearch _securityKeys;
        //TODO private List<ObjectIdentifier> _securityIds

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string securityType, IdentifierSearch securityKeys)
        {
            _pagingRequest = pagingRequest;
            _securityKeys = securityKeys;
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

        public IdentifierSearch SecurityKeys
        {
            get { return _securityKeys; }
        }
    }
}