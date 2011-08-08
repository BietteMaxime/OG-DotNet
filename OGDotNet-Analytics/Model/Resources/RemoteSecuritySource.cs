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
using Fudge.Serialization;
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

        public ISecurity GetSecurity(UniqueId uid)
        {
            var target = _restTarget.Resolve("securities").Resolve("security").Resolve(uid.ToString());
            return target.Get<ISecurity>();
        }

        public ISecurity GetSecurity(ExternalIdBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            Tuple<string, string>[] parameters = UriEncoding.GetParameters(bundle);
            return _restTarget.Resolve("securities").Resolve("security", parameters).Get<SecurityResponse>().Security;
        }

        public IEnumerable<ISecurity> GetBondsWithIssuerName(string issuerName)
        {
            ArgumentChecker.NotNull(issuerName, "issuerName");
            var target = _restTarget.Resolve("bonds", Tuple.Create("issuerName", issuerName));
            return target.Get<SecuritiesResponse>().Securities;
        }

        public ICollection<ISecurity> GetSecurities(ExternalIdBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            var parameters = UriEncoding.GetParameters(bundle);
            return _restTarget.Resolve("securities", parameters).Get<SecuritiesResponse>().Securities;
        }

        private class SecurityResponse
        {
            public ISecurity Security { get; set; }
        }
    }

    internal class SecuritiesResponse
    {
        private readonly IList<ISecurity> _securities;

        public SecuritiesResponse(IList<ISecurity> securities)
        {
            _securities = securities;
        }

        public IList<ISecurity> Securities
        {
            get { return _securities; }
        }
    }
}