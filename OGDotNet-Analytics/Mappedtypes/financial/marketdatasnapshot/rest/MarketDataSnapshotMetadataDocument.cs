//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotMetadataDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;

namespace OGDotNet.Mappedtypes.financial.marketdatasnapshot.rest
{
    public class MarketDataSnapshotMetadataDocument : AbstractDocument
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly string _name;

        private MarketDataSnapshotMetadataDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant, UniqueIdentifier uniqueId, string name) : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _name = name;
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public string Name
        {
            get { return _name; }
        }

        public static MarketDataSnapshotMetadataDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));
            var name = ffc.GetString("name");

            return new MarketDataSnapshotMetadataDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant, uid, name);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
