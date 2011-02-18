using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteSecuritySource : ISecuritySource
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _restTarget;

        public RemoteSecuritySource(OpenGammaFudgeContext fudgeContext, RestTarget restTarget)
        {
            _fudgeContext = fudgeContext;
            _restTarget = restTarget;
        }

        public Security GetSecurity(UniqueIdentifier uid)
        {
            var target = _restTarget.Resolve("securities").Resolve("security").Resolve(uid.ToString());
            return target.Get<Security>();
        }

        public Security GetSecurity(IdentifierBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            Tuple<string, string>[] parameters = UriEncoding.GetParameters(bundle);
            return _restTarget.Resolve("securities").Resolve("security", parameters).Get<Security>("security");
        }

        public ICollection<Security> GetSecurities(IdentifierBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            var parameters = UriEncoding.GetParameters(bundle);
            var fudgeMsg = _restTarget.Resolve("securities", parameters).GetFudge();

            var fudgeSerializer = _fudgeContext.GetSerializer();
            return fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<Security>).ToList();
        }
    }
}