// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionDocumentBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Master.Position;

namespace OpenGamma.Fudge
{
    public class PositionDocumentBuilder : BuilderBase<PositionDocument>
    {
        public PositionDocumentBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override PositionDocument DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionFromInstant;
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            AbstractDocumentHelper.DeserializeVersionCorrection(msg, out versionFromInstant, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (msg.GetString("uniqueId") != null) ? UniqueId.Parse(msg.GetString("uniqueId")) : deserializer.FromField<UniqueId>(msg.GetByName("uniqueId"));
            var position = deserializer.FromField<ManageablePosition>(msg.GetByName("position"));

            return new PositionDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant, uid, position);
        }

        protected override void SerializeImpl(PositionDocument obj, IAppendingFudgeFieldContainer a, IFudgeSerializer serializer)
        {
            AbstractDocumentHelper.SerializeVersionCorrection(obj, a);
            if (obj.UniqueId != null)
            {
                a.Add("uniqueId", obj.UniqueId.ToString());
            }
            a.Add("position", obj.Position);
        }
    }
}
