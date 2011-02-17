using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Master.Security;
using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Model.Resources
{
    /// <summary>
    /// TODO: should I be exposing this
    /// </summary>
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

        public Security GetSecurity(UniqueIdentifier uid)
        {
            SecurityDocument securityDocument = _restTarget.Resolve("security").Resolve(uid.ToString()).Get<SecurityDocument>();
            return securityDocument.Security;
        }
    }
}
