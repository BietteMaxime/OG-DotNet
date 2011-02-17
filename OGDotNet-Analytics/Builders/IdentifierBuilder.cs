using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class IdentifierBuilder : BuilderBase<Identifier>
    {
        public IdentifierBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override Identifier DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string scheme= msg.GetValue<string>("Scheme");
            string value = msg.GetValue<string>("Value");
            return new Identifier(scheme,value);
        }

        protected override void SerializeImpl(Identifier obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add("Scheme",obj.Scheme);
            msg.Add("Value", obj.Value);
        }
    }
}
