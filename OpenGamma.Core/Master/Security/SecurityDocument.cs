// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Core.Security;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Master.Security
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

        public static SecurityDocument FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionFromInstant;
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            AbstractDocumentHelper.DeserializeVersionCorrection(msg, out versionFromInstant, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (msg.GetString("uniqueId") != null) ? UniqueId.Parse(msg.GetString("uniqueId")) : deserializer.FromField<UniqueId>(msg.GetByName("uniqueId"));
            var security = deserializer.FromField<ISecurity>(msg.GetByName("security"));

            return new SecurityDocument(uid, security, versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer s)
        {
            AbstractDocumentHelper.SerializeVersionCorrection(this, msg);
            if (UniqueId != null)
            {
                msg.Add("uniqueId", UniqueId.ToString());
            }
            msg.Add("security", Security);
        }
    }
}