// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePortfolioNodeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Core.Position;
using OpenGamma.Core.Position.Impl;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    class SimplePortfolioNodeBuilder : BuilderBase<SimplePortfolioNode>
    {
        public SimplePortfolioNodeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override SimplePortfolioNode DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new SimplePortfolioNode(
                UniqueId.Parse(msg.GetString("identifier")), msg.GetString("name"), 
                deserializer.FromField<IList<IPortfolioNode>>(msg.GetByName("subNodes")) ?? new List<IPortfolioNode>(), 
                deserializer.FromField<IList<IPosition>>(msg.GetByName("positions")) ?? new List<IPosition>());
        }
    }
}
