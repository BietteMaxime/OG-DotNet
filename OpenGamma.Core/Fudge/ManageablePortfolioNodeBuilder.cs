// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePortfolioNodeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master.Portfolio;

namespace OpenGamma.Fudge
{
    class ManageablePortfolioNodeBuilder : BuilderBase<ManageablePortfolioNode>
    {
        public ManageablePortfolioNodeBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        protected override void SerializeImpl(ManageablePortfolioNode obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (obj.UniqueId != null)
            {
                msg.Add("uniqueId", obj.UniqueId);
            }
            if (obj.PortfolioId != null)
            {
                msg.Add("portfolioId", obj.PortfolioId);
            }
            if (obj.ParentNodeId != null)
            {
                msg.Add("parentNodeId", obj.ParentNodeId);
            }
            serializer.WriteInline(msg, "childNodes", null, obj.ChildNodes);
            serializer.WriteInline(msg, "positionIds", null, obj.PositionIds);
        }

        protected override ManageablePortfolioNode DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var pn = new ManageablePortfolioNode
                         {
                             Name = msg.GetString("name"),
                             ChildNodes = msg.GetMessage("childNodes").Select(deserializer.FromField<ManageablePortfolioNode>).ToList(),
                             PositionIds = msg.GetMessage("positionIds").Select(positionIdField => ObjectId.Parse((string) positionIdField.Value)).ToList()
                         };
            string uniqueIdStr = msg.GetString("uniqueId");
            if (uniqueIdStr != null)
            {
                pn.UniqueId = UniqueId.Parse(uniqueIdStr);
            }
            string portfolioIdStr = msg.GetString("portfolioId");
            if (portfolioIdStr != null)
            {
                pn.PortfolioId = UniqueId.Parse(portfolioIdStr);
            }
            string parentNodeIdStr = msg.GetString("parentNodeId");
            if (parentNodeIdStr != null)
            {
                pn.ParentNodeId = UniqueId.Parse(parentNodeIdStr);
            }
            return pn;
        }
    }
}
