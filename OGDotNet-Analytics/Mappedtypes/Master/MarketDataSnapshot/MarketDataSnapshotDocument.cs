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
            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));

            var versionFromInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("versionFromInstant"));
            var correctionFromInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("correctionFromInstant"));

            var versionToInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("versionToInstant"));

            var correctionToInstant = ToDateTimeOffsetWithDefault(ffc.GetValue<FudgeDateTime>("correctionToInstant"));

            return new MarketDataSnapshotDocument(uid, deserializer.FromField<ManageableMarketDataSnapshot>(ffc.GetByName("snapshot")), versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_uniqueId != null) a.Add("uniqueId", _uniqueId.ToString());
            a.Add("snapshot", _snapshot);

            AddDateTimeOffsetWithDefault(a, "versionFromInstant", VersionFromInstant);
            AddDateTimeOffsetWithDefault(a, "correctionFromInstant", CorrectionFromInstant);
            
            AddDateTimeOffsetWithDefault(a, "versionToInstant", VersionToInstant);
            AddDateTimeOffsetWithDefault(a, "correctionToInstant", CorrectionToInstant);
        }

        private static DateTimeOffset ToDateTimeOffsetWithDefault(FudgeDateTime dt)
        {
            return (dt == null) ? default(DateTimeOffset) : dt.ToDateTimeOffset();
        }

        private static void AddDateTimeOffsetWithDefault(IAppendingFudgeFieldContainer a, string fieldName, DateTimeOffset value)
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
