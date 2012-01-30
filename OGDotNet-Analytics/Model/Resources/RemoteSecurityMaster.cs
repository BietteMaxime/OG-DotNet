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

namespace OGDotNet.Model.Resources
{
    public class RemoteSecurityMaster
    {
        private readonly RestTarget _restTarget;

        public RemoteSecurityMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }
        
        public SearchResult<SecurityDocument> Search(SecuritySearchRequest request)
        {
            return _restTarget.Resolve("securitySearches").Post<SearchResult<SecurityDocument>>(request);
        }

        public ISecurity GetSecurity(UniqueId uid)
        {
            if (uid.IsLatest)
            {
                var securityDocument = _restTarget.Resolve("securities", uid.ToString()).Get<SecurityDocument>();
                return securityDocument.Security;
            }
            else
            {
                var securityDocument = _restTarget.Resolve("securities", uid.ObjectID.ToString(), "versions", uid.Version).Get<SecurityDocument>();
                return securityDocument.Security;
            }
        }

        public SecurityMetaDataResult MetaData(SecurityMetaDataRequest request)
        {
            return _restTarget.Resolve("metaData")
                .WithParam("securityTypes", request.SecurityTypes)
                .Get<SecurityMetaDataResult>();
        }
    }
}
