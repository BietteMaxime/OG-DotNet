using System;
using Fudge;
using Fudge.Serialization;

using OGDotNet_Analytics.Mappedtypes.Core.Position;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Builders
{
    public class PositionBuilder : BuilderBase<Position>
    {
        public PositionBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override Position DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var id = msg.GetValue<string>("identifier");
            var secKey = deserializer.FromField<IdentifierBundle>(msg.GetByName("securityKey"));
            var quant = msg.GetValue<string>("quantity");

            return new Position( UniqueIdentifier.Parse(id), long.Parse(quant), secKey);
        }
    }
}