// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePortfolioBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master.Portfolio;

namespace OpenGamma.Fudge
{
    public class ManageablePortfolioBuilder : BuilderBase<ManageablePortfolio>
    {
        public ManageablePortfolioBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ManageablePortfolio obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (obj.UniqueId != null)
            {
                msg.Add("uniqueId", obj.UniqueId);
            }
            if (obj.Name != null)
            {
                msg.Add("name", obj.Name);
            }
            serializer.WriteInline(msg, "rootNode", null, obj.RootNode);
            serializer.WriteInline(msg, "attributes", null, obj.Attributes);
        }

        protected override ManageablePortfolio DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var p = new ManageablePortfolio
                        {
                            Name = msg.GetString("name"),
                            RootNode = deserializer.FromField<ManageablePortfolioNode>(msg.GetByName("rootNode")),
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
