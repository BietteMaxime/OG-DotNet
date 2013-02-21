// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Model;
using OpenGamma.Util;

namespace OpenGamma.Financial
{
    public class RemoteMaster<TDocument, TSearchRequest, THistoryRequest>
        where TDocument : AbstractDocument
        where TSearchRequest : class
        where THistoryRequest : AbstractHistoryRequest
    {
        private readonly MasterRestTarget _rest;

        public RemoteMaster(RestTarget rest, string resourceName, string searchResourceName)
        {
            _rest = new MasterRestTarget(rest, resourceName, searchResourceName);
        }

        public SearchResult<TDocument> Search(TSearchRequest request)
        {
            ArgumentChecker.NotNull(request, "request");
            return _rest.GetRestSearch().Post<SearchResult<TDocument>>(request);
        }

        public TDocument Get(UniqueId uniqueId)
        {
            ArgumentChecker.NotNull(uniqueId, "uniqueId");
            if (uniqueId.IsLatest)
            {
                return Get(uniqueId.ObjectId, VersionCorrection.Latest);
            }
            return _rest.GetRestUid(uniqueId).Get<TDocument>();
        }

        public TDocument Get(ObjectId objectId, VersionCorrection versionCorrection)
        {
            ArgumentChecker.NotNull(objectId, "objectId");
            return _rest.GetRestOidVc(objectId, versionCorrection).Get<TDocument>();
        }

        public TDocument Add(TDocument document)
        {
            ArgumentChecker.NotNull(document, "document");
            return _rest.GetRestMain().Post<TDocument>(document);
        }

        public TDocument Update(TDocument document)
        {
            ArgumentChecker.NotNull(document, "document");
            ArgumentChecker.NotNull(document.UniqueId, "document.UniqueId");
            return _rest.GetRestOidVc(document.UniqueId, null).Post<TDocument>(document);
        }

        public TDocument Correct(TDocument document)
        {
            ArgumentChecker.NotNull(document, "document");
            ArgumentChecker.NotNull(document.UniqueId, "document.UniqueId");
            return _rest.GetRestUid(document.UniqueId).Post<TDocument>(document);
        }

        public void Remove(IObjectIdentifiable objectIdentifiable)
        {
            ArgumentChecker.NotNull(objectIdentifiable, "objectIdentifiable");
            _rest.GetRestOidVc(objectIdentifiable.ObjectId, null).Delete();
        }

        public SearchResult<TDocument> History(THistoryRequest request)
        {
            return MasterRestTarget.GetRestOidHistory(request).Get<SearchResult<TDocument>>();
        }

        protected MasterRestTarget MasterRestTarget
        {
            get
            {
                return _rest;
            }
        }
    }
}
