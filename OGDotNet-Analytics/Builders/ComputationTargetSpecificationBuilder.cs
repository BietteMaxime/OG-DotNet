using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Mappedtypes.engine;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Builders
{
    public class ComputationTargetSpecificationBuilder : BuilderBase<ComputationTargetSpecification>
    {
        public ComputationTargetSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ComputationTargetSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            ComputationTargetType type;
            if (! Enum.TryParse(msg.GetValue<String>("computationTargetType"), out type))
            {
                throw new Exception("Unhandled computation target type");
            }
            UniqueIdentifier uid = null;
            var ctiField = msg.GetByName("computationTargetIdentifier");
            if (ctiField !=null) {
                uid = UniqueIdentifier.Parse(msg.GetValue<String>("computationTargetIdentifier"));
            }
            return new ComputationTargetSpecification(type, uid);
        }
    }
}