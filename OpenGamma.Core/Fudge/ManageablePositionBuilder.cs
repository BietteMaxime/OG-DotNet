// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePositionBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master.Position;
using OpenGamma.Master.Security;

namespace OpenGamma.Fudge
{
    public class ManageablePositionBuilder : BuilderBase<ManageablePosition>
    {
        public ManageablePositionBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ManageablePosition obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (obj.UniqueId != null)
            {
                msg.Add("uniqueId", obj.UniqueId);
            }
            msg.Add("quantity", obj.Quantity.ToString());
            serializer.WriteInline(msg, "securityLink", obj.SecurityLink);
            serializer.WriteInline(msg, "attributes", obj.Attributes);
        }

        protected override ManageablePosition DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string quantityStr = msg.GetString("quantity");
            decimal quantity;
            decimal.TryParse(quantityStr, out quantity);

            var p = new ManageablePosition
            {
                Quantity = quantity,
                SecurityLink = deserializer.FromField<ManageableSecurityLink>(msg.GetByName("securityLink")),
                Attributes = MapBuilder.FromFudgeMsg<string, string>(msg.GetMessage("attributes"), deserializer)
            };
            string uniqueIdStr = msg.GetString("uniqueId");
            if (uniqueIdStr != null)
            {
                p.UniqueId = UniqueId.Parse(uniqueIdStr);
            }
            return p;
        }
    }
}
