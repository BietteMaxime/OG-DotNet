using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;

using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public class RemoteSecuritySource : ISecuritySource
    {
        private readonly RestTarget _restTarget;

        public RemoteSecuritySource(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public Security GetSecurity(UniqueIdentifier uid)
        {
            var fudgeMsg = _restTarget.Resolve("securities").Resolve("security").Resolve(uid.ToString()).GetReponse();
            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            return fudgeSerializer.Deserialize<Security> (fudgeMsg); 
        }

        public Security GetSecurity(IdentifierBundle bundle)
        {
            Tuple<string, string>[] parameters = GetParameters(bundle);
            var fudgeMsg = _restTarget.Resolve("securities").Resolve("security", parameters).GetReponse();

            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            return fudgeSerializer.Deserialize<Security>((FudgeMsg) fudgeMsg.GetMessage("security"));

        }

        public ICollection<Security> GetSecurities(IdentifierBundle bundle)
        {

            var parameters = GetParameters(bundle);
            var fudgeMsg = _restTarget.Resolve("securities", parameters).GetReponse();

            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            return fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<Security>).ToList();
        }

        private static Tuple<string, string>[] GetParameters(IdentifierBundle bundle)
        {
            var ids = bundle.Identifiers.ToList();

            return ids.Select(s => new Tuple<string, string>("id", s.ToString())).ToArray();
        }
    }
}