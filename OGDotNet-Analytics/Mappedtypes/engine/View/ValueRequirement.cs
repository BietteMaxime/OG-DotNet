using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ValueRequirement
    {
        private readonly string _valueName;
        private readonly ComputationTargetType _computationTargetType;
        private readonly UniqueIdentifier _computationTargetIdentifier;

        public string ValueName { get { return _valueName; }}
        public ComputationTargetType ComputationTargetType { get { return _computationTargetType; } }
        public UniqueIdentifier ComputationTargetIdentifier { get { return _computationTargetIdentifier; } }

        private ValueRequirement(string valueName,string computationTargetType, string computationTargetIdentifier)
        {
            _valueName = valueName;
            _computationTargetType = ComputationTargetTypeBuilder.GetComputationTargetType(computationTargetType);
            _computationTargetIdentifier = UniqueIdentifier.Parse(computationTargetIdentifier);
        }

        private ComputationTargetSpecification GetTargetSpec()
        {
            return new ComputationTargetSpecification(ComputationTargetType, ComputationTargetIdentifier);
        }
        public ValueSpecification ToSpecification()
        {
            return new ValueSpecification(ValueName, GetTargetSpec());
        }

        public static ValueRequirement FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ValueRequirement(ffc.GetValue<string>("valueName"), ffc.GetValue<string>("computationTargetType"), ffc.GetValue<string>("computationTargetIdentifier"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}