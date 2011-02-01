using System;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Mappedtypes.Util.Db;

namespace OGDotNet_Analytics.Mappedtypes.Master.Security
{
    [Serializable]
    internal class SecuritySearchRequest
    {
        public readonly PagingRequest PagingRequest;
        public readonly string Name;
        public readonly string SecurityType;
        public readonly IdentifierSearch SecurityKeys;
        //private List<ObjectIdentifier> _securityIds

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string securityType, IdentifierSearch securityKeys)
        {
            PagingRequest = pagingRequest;
            SecurityKeys = securityKeys;
            Name = name;
            SecurityType = securityType ?? "";
        }
    }
}