//-----------------------------------------------------------------------
// <copyright file="RemoteSecuritySource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.FudgeMsg;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteSecuritySource : IFinancialSecuritySource
    {
        private readonly RestTarget _restTarget;

        public RemoteSecuritySource(RestTarget restTarget)
        {
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
            return _restTarget.Resolve("securitySearches").Resolve("single", parameters).Get<ISecurity>();
        }

        public ICollection<ISecurity> GetSecurities(ExternalIdBundle bundle)
        {
            ArgumentChecker.NotEmpty(bundle.Identifiers, "bundle");

            var parameters = UriEncoding.GetParameters(bundle);
            return _restTarget.Resolve("securities", parameters).Get<FudgeListWrapper<ISecurity>>().List;
        }
    }
}