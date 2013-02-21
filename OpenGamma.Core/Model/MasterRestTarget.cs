// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MasterRestTarget.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Model.Resources;

namespace OpenGamma.Model
{
    public class MasterRestTarget
    {
        private readonly RestTarget _rest;

        private readonly string _resourceName;

        private readonly string _searchResourceName;

        public MasterRestTarget(RestTarget rest, string resourceName, string searchResourceName)
        {
            _rest = rest;
            _resourceName = resourceName;
            _searchResourceName = searchResourceName;
        }

        public RestTarget GetRestBase()
        {
            return _rest;
        }

        public RestTarget GetRestMain()
        {
            return _rest.Resolve(_resourceName);
        }

        public RestTarget GetRestSearch()
        {
            return _rest.Resolve(_searchResourceName);
        }

        public RestTarget GetRestUid(UniqueId uniqueId)
        {
            return GetRestMain().Resolve(uniqueId.ObjectId.ToString()).Resolve("versions").Resolve(uniqueId.Version);
        }

        public RestTarget GetRestOidVc(IObjectIdentifiable objectIdentifiable, VersionCorrection versionCorrection)
        {
            RestTarget target = GetRestMain().Resolve(objectIdentifiable.ObjectId.ToString());
            if (versionCorrection != null)
            {
                target = target.WithParam("versionAsOf", versionCorrection.VersionAsOfString)
                               .WithParam("correctedTo", versionCorrection.CorrectedToString);
            }
            return target;
        }

        public RestTarget GetRestOidHistory(AbstractHistoryRequest request)
        {
            RestTarget target = GetRestMain().Resolve(request.ObjectId.ToString()).Resolve("versions");
            return RestUtils.EncodeQueryParams(target, request);
        }
    }
}
