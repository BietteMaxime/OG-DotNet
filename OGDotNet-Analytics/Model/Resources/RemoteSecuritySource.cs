//-----------------------------------------------------------------------
// <copyright file="RemoteSecuritySource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteSecuritySource : IFinancialSecuritySource
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _restTarget;

        public RemoteSecuritySource(OpenGammaFudgeContext fudgeContext, RestTarget restTarget)
        {
            _fudgeContext = fudgeContext;
            _restTarget = restTarget;
        }

        public ISecurity GetSecurity(UniqueIdentifier uid)
        {
            var target = _restTarget.Resolve("securities").Resolve("security").Resolve(uid.ToString());
            return target.Get<ISecurity>();
        }

        public ISecurity GetSecurity(IdentifierBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            Tuple<string, string>[] parameters = UriEncoding.GetParameters(bundle);
            return _restTarget.Resolve("securities").Resolve("security", parameters).Get<ISecurity>("security");
        }

        public IEnumerable<ISecurity> GetBondsWithIssuerName(string issuerName)
        {
            ArgumentChecker.NotNull(issuerName, "issuerName");
            var target = _restTarget.Resolve("bonds", Tuple.Create("issuerName", issuerName));
            var msg = target.GetFudge();
            var fudgeSerializer = _fudgeContext.GetSerializer();
            return msg.GetAllByName("security").Select(field => fudgeSerializer.Deserialize<ISecurity>((FudgeMsg) field.Value)).ToList();
        }

        public ICollection<ISecurity> GetSecurities(IdentifierBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            var parameters = UriEncoding.GetParameters(bundle);
            var fudgeMsg = _restTarget.Resolve("securities", parameters).GetFudge();

            var fudgeSerializer = _fudgeContext.GetSerializer();
            return fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<ISecurity>).ToList();
        }
    }
}