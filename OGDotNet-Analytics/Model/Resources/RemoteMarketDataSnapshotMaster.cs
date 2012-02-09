//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Model.Resources
{
    public class RemoteMarketDataSnapshotMaster : IMaster<MarketDataSnapshotDocument>
    {
        private readonly RestTarget _restTarget;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemoteMarketDataSnapshotMaster(RestTarget restTarget, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _fudgeContext = fudgeContext;
        }

        public SearchResult<MarketDataSnapshotDocument> Search(string name, PagingRequest pagingRequest)
        {
            var request = new MarketDataSnapshotSearchRequest(name, pagingRequest);
            return Search(request);
        }

        public SearchResult<MarketDataSnapshotDocument> Search(MarketDataSnapshotSearchRequest request)
        {
            return _restTarget.Resolve("snapshotSearches").Post<SearchResult<MarketDataSnapshotDocument>>(request);
        }

        public MarketDataSnapshotDocument Add(MarketDataSnapshotDocument document)
        {
            var r = _restTarget.Resolve("snapshots").Post<MarketDataSnapshotDocument>(document);
            return Update(document, r);
        }

        public MarketDataSnapshotDocument Update(MarketDataSnapshotDocument document)
        {
            if (document.UniqueId == null)
            {
                throw new ArgumentException();
            }
            var r = _restTarget.Resolve("snapshots", document.UniqueId.ObjectID.ToString()).Post<MarketDataSnapshotDocument>(document);
            return Update(document, r);
        }

        private static MarketDataSnapshotDocument Update(MarketDataSnapshotDocument req, MarketDataSnapshotDocument resp)
        {
            req.UniqueId = resp.UniqueId;
            req.Snapshot.UniqueId = resp.UniqueId;
            return req;
        }

        public RemoteChangeManger ChangeManager
        {
            get
            {
                return new RemoteChangeManger(_restTarget.Resolve("snapshots", "changeManager"), _fudgeContext);
            }
        }

        public MarketDataSnapshotDocument Get(UniqueId uniqueId)
        {
            var target = _restTarget.Resolve("snapshots").Resolve(uniqueId.ObjectID.ToString());
            if (uniqueId.IsVersioned)
            {
                target = target.Resolve("versions", uniqueId.Version);
            }
            var resp = target.Get<MarketDataSnapshotDocument>();
            if (resp == null || resp.UniqueId == null || resp.Snapshot == null)
            {
                throw new ArgumentException("Not found", "uniqueId");
            }
            return resp;
        }

        public void Remove(UniqueId uniqueId)
        {
            _restTarget.Resolve("snapshots").Resolve(uniqueId.ObjectID.ToString()).Delete();
        }
        //TODO correct

        public MarketDataSnapshotHistoryResult History(MarketDataSnapshotHistoryRequest request)
        {
            var versionsTarget = _restTarget.Resolve("snapshots", request.ObjectId.ToString(), "versions");
            return RestUtils.EncodeQueryParams(versionsTarget, request)
                .Get<MarketDataSnapshotHistoryResult>();
        }
    }
}