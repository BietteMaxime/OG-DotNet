//-----------------------------------------------------------------------
// <copyright file="SecurityDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Security
{
    public class SecurityDocument : AbstractDocument
    {
        private readonly ISecurity _security;
        private UniqueId _uniqueId;

        public override UniqueId UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public ISecurity Security { get { return _security; } }

        public SecurityDocument(UniqueId uniqueId, ISecurity security, DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant)
            : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _security = security;
        }

        public override string ToString()
        {
            return Security.ToString();
        }

        public static SecurityDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueId.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));
            var security = deserializer.FromField<ISecurity>(ffc.GetByName("security"));

            return new SecurityDocument(uid, security, versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            WriteDocumentFields(a);

            if (UniqueId != null)
            {
                a.Add("uniqueId", UniqueId.ToString());
            }
            a.Add("security", Security);
        }
    }
}