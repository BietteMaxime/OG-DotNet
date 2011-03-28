using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Model.Resources
{
    public class RemoteMarketDataSnapshotMaster
    {
        private readonly RestTarget _restTarget;

        public RemoteMarketDataSnapshotMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public SearchResult<MarketDataSnapshotDocument> Search(string name, PagingRequest pagingRequest)
        {
            var request = new MarketDataSnapshotSearchRequest(pagingRequest, name);
            return _restTarget.Resolve("search").Post<SearchResult<MarketDataSnapshotDocument>>(request);
        }

        public MarketDataSnapshotDocument Add(MarketDataSnapshotDocument document)
        {
            return PostDefinition(document, "add");
        }


        private MarketDataSnapshotDocument PostDefinition(MarketDataSnapshotDocument document, params string[] pathParts)
        {
            if (document.UniqueId != null)
            {
                document.UniqueId = document.UniqueId.ToLatest();
            }
            var target =  pathParts.Aggregate(_restTarget, (r, p) => r.Resolve(p));
            var respMsg = target.Post<UniqueIdentifier>(document, "uniqueId");
            var uid = respMsg.UniqueId;
            if (uid == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = uid;

            return document;
        }

        public MarketDataSnapshotDocument Get(UniqueIdentifier uniqueId)
        {
            var resp = _restTarget.Resolve("snapshots").Resolve(uniqueId.ToString()).Get<MarketDataSnapshotDocument>();
            if (resp == null || resp.UniqueId == null || resp.Snapshot == null)
            {
                throw new ArgumentException("Not found", "uniqueId");
            }
            return resp;
        }

        public void Remove(UniqueIdentifier uniqueId)
        {
            _restTarget.Resolve("snapshots").Resolve(uniqueId.ToString()).Delete();
        }
        //TODO Update, correct

    }
}