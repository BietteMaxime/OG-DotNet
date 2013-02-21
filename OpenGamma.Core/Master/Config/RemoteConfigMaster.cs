// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteConfigMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Model;
using OpenGamma.Util;

namespace OpenGamma.Master.Config
{
    public class RemoteConfigMaster
    {
        private readonly MasterRestTarget _rest;

        public RemoteConfigMaster(RestTarget rest)
        {
            _rest = new MasterRestTarget(rest, "configs", "configSearches");
        }

        public SearchResult<ConfigDocument<T>> Search<T>(ConfigSearchRequest request)
        {
            ArgumentChecker.NotNull(request, "request");
            return _rest.GetRestSearch().Post<SearchResult<ConfigDocument<T>>>(request);
        }

        public ConfigDocument<T> Get<T>(UniqueId uniqueId)
        {
            ArgumentChecker.NotNull(uniqueId, "uniqueId");
            if (uniqueId.IsLatest)
            {
                return Get<T>(uniqueId.ObjectId, VersionCorrection.Latest);
            }
            return _rest.GetRestUid(uniqueId).Get<ConfigDocument<T>>();
        }

        public ConfigDocument<T> Get<T>(ObjectId objectId, VersionCorrection versionCorrection)
        {
            ArgumentChecker.NotNull(objectId, "objectId");
            return _rest.GetRestOidVc(objectId, versionCorrection).Get<ConfigDocument<T>>();
        }

        public ConfigDocument<T> Add<T>(ConfigDocument<T> document)
        {
            ArgumentChecker.NotNull(document, "document");
            return _rest.GetRestMain().Post<ConfigDocument<T>>(document);
        }

        public ConfigDocument<T> Update<T>(ConfigDocument<T> document)
        {
            ArgumentChecker.NotNull(document, "document");
            ArgumentChecker.NotNull(document.UniqueId, "document.UniqueId");
            return _rest.GetRestOidVc(document.UniqueId, null).Post<ConfigDocument<T>>(document);
        }

        public ConfigDocument<T> Correct<T>(ConfigDocument<T> document)
        {
            ArgumentChecker.NotNull(document, "document");
            ArgumentChecker.NotNull(document.UniqueId, "document.UniqueId");
            return _rest.GetRestUid(document.UniqueId).Post<ConfigDocument<T>>(document);
        }

        public void Remove(IObjectIdentifiable objectIdentifiable)
        {
            ArgumentChecker.NotNull(objectIdentifiable, "objectIdentifiable");
            _rest.GetRestOidVc(objectIdentifiable.ObjectId, null).Delete();
        }
    }
}
