using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;

namespace OGDotNet.Builders
{
    class PortfolioBuilder : BuilderBase<IPortfolio>
    {
        public PortfolioBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override IPortfolio DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new PortfolioImpl(deserializer.FromField<PortfolioNode>(msg.GetByName("root")), msg.GetString("identifier"), msg.GetString("name"));
        }
    }
}
