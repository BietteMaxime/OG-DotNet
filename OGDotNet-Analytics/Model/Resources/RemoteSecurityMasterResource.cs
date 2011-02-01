using System.Collections.Generic;
using System.Linq;
using Fudge;
using OGDotNet_Analytics.SecurityExplorer;

namespace OGDotNet_Analytics.Model.Resources
{
    class RemoteSecurityMasterResource 
    {
        private readonly RestTarget _restTarget;

        public RemoteSecurityMasterResource(string baseUri)
        {
            _restTarget = new RestTarget(baseUri).GetSubMagic("securityMaster");
        }

        public IEnumerable<RemoteSecurityMaster> GetSecurityMasters()
        {
            FudgeMsg reponse = _restTarget.GetReponse();
            IList<IFudgeField> fudgeFields = reponse.GetAllFields();

            return fudgeFields.Select(fudgeField => (string)fudgeField.Value).Select(GetSecurityMaster);
        }

        public RemoteSecurityMaster GetSecurityMaster(string uid)
        {
            return new RemoteSecurityMaster(_restTarget.GetSubMagic(uid));
        }


    }
}