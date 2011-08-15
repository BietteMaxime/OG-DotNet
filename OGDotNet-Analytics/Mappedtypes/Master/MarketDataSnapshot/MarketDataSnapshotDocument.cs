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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class MarketDataSnapshotDocument : AbstractDocument
    {
        private UniqueId _uniqueId;
        private ManageableMarketDataSnapshot _snapshot;

        public override UniqueId UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get { return _snapshot; }
            set { _snapshot = value; }
        }

        private MarketDataSnapshotDocument(UniqueId uniqueId, ManageableMarketDataSnapshot snapshot, DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant) : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _snapshot = snapshot;
        }

        public MarketDataSnapshotDocument(UniqueId uniqueId, ManageableMarketDataSnapshot snapshot)
        {
            _uniqueId = uniqueId;
            _snapshot = snapshot;
        }

        public static MarketDataSnapshotDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueId.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));
            var snapshot = deserializer.FromField<ManageableMarketDataSnapshot>(ffc.GetByName("snapshot"));

            return new MarketDataSnapshotDocument(uid, snapshot, versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            WriteDocumentFields(a);

            if (_uniqueId != null)
            {
                a.Add("uniqueId", _uniqueId.ToString());
            }
            a.Add("snapshot", _snapshot);
        }

        public override string ToString()
        {
            return Snapshot.ToString();
        }
    }
}
