//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotDocument : AbstractDocument
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

        private MarketDataSnapshotDocument(UniqueIdentifier uniqueId, ManageableMarketDataSnapshot snapshot, DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant) : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _snapshot = snapshot;
        }

        public MarketDataSnapshotDocument(UniqueIdentifier uniqueId, ManageableMarketDataSnapshot snapshot)
        {
            _uniqueId = uniqueId;
            _snapshot = snapshot;
        }

        public static MarketDataSnapshotDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var uid = ffc.GetValue<UniqueIdentifier>("uniqueId");

            var versionFromInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("versionFromInstant"));
            var correctionFromInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("correctionFromInstant"));

            var vToInst = ffc.GetValue<FudgeDateTime>("versionToInstant");
            var versionToInstant = ToDateTimeOffsetWithDefault(vToInst);

            var cToInst = ffc.GetValue<FudgeDateTime>("correctionToInstant");
            var correctionToInstant = ToDateTimeOffsetWithDefault(cToInst);

            return new MarketDataSnapshotDocument(uid, deserializer.FromField<ManageableMarketDataSnapshot>(ffc.GetByName("snapshot")), versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant);
        }

        private static DateTimeOffset ToDateTimeOffsetWithDefault(FudgeDateTime dt)
        {
            return (dt == null ) ? default(DateTimeOffset)  : dt.ToDateTimeOffset();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_uniqueId != null) a.Add("uniqueId", _uniqueId.ToString());
            a.Add("snapshot", _snapshot);

            AddField(a, VersionFromInstant, "versionFromInstant");
            AddField(a, CorrectionFromInstant, "correctionFromInstant");
            AddField(a, VersionToInstant, "versionToInstant");
            AddField(a, CorrectionToInstant, "correctionToInstant");
        }

        private static void AddField(IAppendingFudgeFieldContainer a, DateTimeOffset value, string fieldName)
        {
            if (value != default(DateTimeOffset))
            {
                a.Add(fieldName, new FudgeDateTime(value));
            }
        }

        public override string ToString()
        {
            return Snapshot.ToString();
        }
    }
}
