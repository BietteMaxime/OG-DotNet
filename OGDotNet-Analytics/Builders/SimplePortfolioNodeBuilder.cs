//-----------------------------------------------------------------------
// <copyright file="SimplePortfolioNodeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class SimplePortfolioNodeBuilder : BuilderBase<SimplePortfolioNode>
    {
        public SimplePortfolioNodeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override SimplePortfolioNode DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new SimplePortfolioNode(
                UniqueId.Parse(msg.GetString("identifier")), msg.GetString("name"),
                deserializer.FromField<IList<PortfolioNode>>(msg.GetByName("subNodes")) ?? new List<PortfolioNode>(),
                deserializer.FromField<IList<IPosition>>(msg.GetByName("positions")) ?? new List<IPosition>());
        }
    }
}
