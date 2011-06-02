//-----------------------------------------------------------------------
// <copyright file="RemoteSecurityMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Master.Security;
using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Model.Resources
{
    public class RemoteSecurityMaster
    {
        private readonly RestTarget _restTarget;

        public RemoteSecurityMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }
        
        public SearchResult<SecurityDocument> Search(string name, string type, PagingRequest pagingRequest, IdentifierSearch identifierSearch = null)
        {
            var request = new SecuritySearchRequest(pagingRequest, name, type, identifierSearch);
            return _restTarget.Resolve("search").Post<SearchResult<SecurityDocument>>(request);
        }

        public ISecurity GetSecurity(UniqueIdentifier uid)
        {
            SecurityDocument securityDocument = _restTarget.Resolve("security").Resolve(uid.ToString()).Get<SecurityDocument>();
            return securityDocument.Security;
        }

        public SecurityMetaDataResult MetaData(SecurityMetaDataRequest request)
        {
            return _restTarget.Resolve("metaData").Post<SecurityMetaDataResult>(request);
        }
    }
}
