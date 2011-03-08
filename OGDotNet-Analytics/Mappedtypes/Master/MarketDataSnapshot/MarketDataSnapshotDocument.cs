using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotDocument
    {
        private UniqueIdentifier _uniqueId;
        private ManageableMarketDataSnapshot _snapshot;

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get { return _snapshot; }
            set { _snapshot = value; }
        }

        public MarketDataSnapshotDocument()
        {
        }

        public MarketDataSnapshotDocument(UniqueIdentifier uniqueId, ManageableMarketDataSnapshot snapshot)
        {
            _uniqueId = uniqueId;
            _snapshot = snapshot;
        }

        public static MarketDataSnapshotDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));
            return new MarketDataSnapshotDocument(uid, deserializer.FromField<ManageableMarketDataSnapshot>(ffc.GetByName("snapshot")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_uniqueId != null) a.Add("uniqueId", _uniqueId.ToString());
            a.Add("snapshot", _snapshot);
        }

        public override string ToString()
        {
            return Snapshot.ToString();
        }
    }
}
