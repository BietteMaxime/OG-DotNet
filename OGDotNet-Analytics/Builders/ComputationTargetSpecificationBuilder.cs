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
            ComputationTargetType type = EnumBuilder<ComputationTargetType>.Parse(msg.GetValue<String>("computationTargetType"));
            UniqueIdentifier uid = null;
            var ctiField = msg.GetByName("computationTargetIdentifier");
            if (ctiField !=null) {
                uid = UniqueIdentifier.Parse(msg.GetValue<String>("computationTargetIdentifier"));
            }
            return new ComputationTargetSpecification(type, uid);
        }

        public static void AddMessageFields(IFudgeSerializer fudgeSerializer, IAppendingFudgeFieldContainer msg, ComputationTargetSpecification @object)
        {
            msg.Add("computationTargetType", EnumBuilder<ComputationTargetType>.GetJavaName(@object.Type));
            UniqueIdentifier uid = @object.Uid;
            if (uid != null)
            {
                fudgeSerializer.WriteInline(msg, "computationTargetIdentifier", uid);
            }
        }
    }
}