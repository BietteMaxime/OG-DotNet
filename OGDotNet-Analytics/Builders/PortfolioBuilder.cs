// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortfolioBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class PortfolioBuilder : BuilderBase<IPortfolio>
    {
        public PortfolioBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override IPortfolio DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new SimplePortfolio(deserializer.FromField<PortfolioNode>(msg.GetByName("root")), UniqueId.Parse(msg.GetString("identifier")), msg.GetString("name"));
        }
    }
}
