// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteSecurityMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Financial;
using OpenGamma.Master.Security;

namespace OpenGamma.Model.Resources
{
    public class RemoteSecurityMaster : RemoteMaster<SecurityDocument, SecuritySearchRequest, SecurityHistoryRequest>
    {
        public RemoteSecurityMaster(RestTarget restTarget) : base(restTarget, "securities", "securitySearches")
        {
        }
        
        public SecurityMetaDataResult MetaData(SecurityMetaDataRequest request)
        {
            var restTarget = MasterRestTarget.GetRestBase().Resolve("metaData");
            return RestUtils.EncodeQueryParams(restTarget, request).Get<SecurityMetaDataResult>();
        }
    }
}
