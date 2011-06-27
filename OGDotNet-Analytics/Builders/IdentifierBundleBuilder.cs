using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class IdentifierBundleBuilder : BuilderBase<IdentifierBundle>
    {
        public IdentifierBundleBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override IdentifierBundle DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var identifiers = new HashSet<Identifier>();

            foreach (var field in msg.GetAllFields())
            {
                switch (field.Name)
                {
                    case "ID":
                        var i = (Identifier)deserializer.FromField(field, typeof(Identifier));
                        identifiers.Add(i);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new IdentifierBundle(identifiers);
        }

        protected override void SerializeImpl(IdentifierBundle obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            foreach (var identifier in obj.Identifiers)
            {
                msg.Add("ID", identifier);
            }
        }
    }
}
