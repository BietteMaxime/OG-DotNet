using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Mappedtypes.Core.Security;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Mappedtypes.Master;
using OGDotNet_Analytics.Mappedtypes.Master.Security;
using OGDotNet_Analytics.Mappedtypes.Util.Db;

namespace OGDotNet_Analytics.Model.Resources
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
        
        public AbstractSearchResult<SecurityDocument> Search(string name, string type, PagingRequest pagingRequest, IdentifierSearch identifierSearch = null)
        {
            var request = new SecuritySearchRequest(pagingRequest, name, type, identifierSearch);

            var fudgeSerializer = new FudgeSerializer(FudgeContext);
            var msg = fudgeSerializer.SerializeToMsg(request);
            var fudgeMsg = _restTarget.Resolve("search").Post(FudgeContext, msg);


            return fudgeSerializer.Deserialize<AbstractSearchResult<SecurityDocument>>(fudgeMsg); 
        }

        private static FudgeContext FudgeContext
        {
            get
            {
                return FudgeConfig.GetFudgeContext();
            }
        }

        public Security GetSecurity(UniqueIdentifier uid)
        {
            var fudgeMsg = _restTarget.Resolve("security").Resolve(uid.ToString()).GetReponse();
            FudgeSerializer fudgeSerializer = new FudgeSerializer(FudgeContext);
            return fudgeSerializer.Deserialize<SecurityDocument>(fudgeMsg).Security;

        }
    }
}
