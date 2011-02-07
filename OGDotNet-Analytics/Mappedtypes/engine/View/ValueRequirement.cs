using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ValueRequirement
    {
        private readonly string _valueName;
        private readonly ComputationTargetType _computationTargetType;
        private readonly UniqueIdentifier _computationTargetIdentifier;
        private readonly ValueProperties _constraints;

        public string ValueName { get { return _valueName; } }
        public ComputationTargetType ComputationTargetType { get { return _computationTargetType; } }
        public UniqueIdentifier ComputationTargetIdentifier { get { return _computationTargetIdentifier; } }
        public ValueProperties Constraints { get { return _constraints; } }
        private ComputationTargetSpecification TargetSpecification
        {
            get { return new ComputationTargetSpecification(ComputationTargetType, ComputationTargetIdentifier); }
        }

        

        private ValueRequirement(string valueName,string computationTargetType, string computationTargetIdentifier, ValueProperties constraints)
        {
            _valueName = valueName;
            _constraints = constraints;
            _computationTargetType = ComputationTargetTypeBuilder.GetComputationTargetType(computationTargetType);
            _computationTargetIdentifier = UniqueIdentifier.Parse(computationTargetIdentifier);
        }



        public static ValueRequirement FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            ValueProperties constraints = deserializer.FromField<ValueProperties>(ffc.GetByName("constraints")) ?? new ValueProperties();
            return new ValueRequirement(ffc.GetValue<string>("valueName"), ffc.GetValue<string>("computationTargetType"), ffc.GetValue<string>("computationTargetIdentifier"), constraints);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}