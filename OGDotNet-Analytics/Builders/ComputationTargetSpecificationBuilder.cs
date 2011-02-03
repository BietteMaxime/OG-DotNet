using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    internal class ComputationTargetSpecificationBuilder : BuilderBase<ComputationTargetSpecification>
    {
        public ComputationTargetSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ComputationTargetSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            ComputationTargetType type = ComputationTargetTypeBuilder.GetComputationTargetType(msg.GetValue<String>("computationTargetType"));
            UniqueIdentifier uid = null;
            var ctiField = msg.GetByName("computationTargetIdentifier");
            if (ctiField !=null) {
                uid = UniqueIdentifier.Parse(msg.GetValue<String>("computationTargetIdentifier"));
            }
            return new ComputationTargetSpecification(type, uid);
        }
    }
}