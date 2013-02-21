// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortfolioBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Core.Position;
using OpenGamma.Core.Position.Impl;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    class PortfolioBuilder : BuilderBase<IPortfolio>
    {
        public PortfolioBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override IPortfolio DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new SimplePortfolio(deserializer.FromField<SimplePortfolioNode>(msg.GetByName("root")), UniqueId.Parse(msg.GetString("identifier")), msg.GetString("name"));
        }
    }
}
