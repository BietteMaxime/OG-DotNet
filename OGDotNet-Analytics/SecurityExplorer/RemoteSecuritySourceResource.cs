using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Mappedtypes.Master;
using OGDotNet_Analytics.Mappedtypes.Master.Security;
using OGDotNet_Analytics.Mappedtypes.Util.Db;
using OGDotNet_Analytics.Model;

namespace OGDotNet_Analytics.SecurityExplorer
{
    public class RemoteSecurityMaster
    {
        internal readonly RestTarget _restTarget;

        public RemoteSecurityMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }
        
        public AbstractSearchResult<SecurityDocument> Search(string name, string type, PagingRequest pagingRequest, IdentifierSearch identifierSearch)
        {
            var request = new SecuritySearchRequest(pagingRequest, name, type, identifierSearch);

            FudgeSerializer fudgeSerializer = new FudgeSerializer(FudgeContext);
            var msg = fudgeSerializer.SerializeToMsg(request);
            var fudgeMsg = _restTarget.GetSubMagic("search").GetReponse(FudgeContext, msg);


            return fudgeSerializer.Deserialize<AbstractSearchResult<SecurityDocument>>(fudgeMsg); 
        }
        public AbstractSearchResult<SecurityDocument> Search(string name, string type, PagingRequest pagingRequest)
        {
            return Search(name, type, pagingRequest, null);
        }

        private static FudgeContext FudgeContext
        {
            get
            {
                return FudgeConfig.GetFudgeContext();
            }
        }

        public ManageableSecurity GetSecurity(UniqueIdentifier uid)
        {
            var fudgeMsg = _restTarget.GetSubMagic("security").GetSubMagic(uid.ToString()).GetReponse();
            FudgeSerializer fudgeSerializer = new FudgeSerializer(FudgeContext);
            return fudgeSerializer.Deserialize<SecurityDocument>(fudgeMsg).Security;

        }
    }
}
