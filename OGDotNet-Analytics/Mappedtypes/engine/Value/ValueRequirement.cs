//-----------------------------------------------------------------------
// <copyright file="ValueRequirement.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.value
{
    public class ValueRequirement
    {
        private readonly string _valueName;
        private readonly ComputationTargetSpecification _targetSpecification;

        private readonly ValueProperties _constraints;

        public string ValueName { get { return _valueName; } }


        public ValueProperties Constraints { get { return _constraints; } }
        public ComputationTargetSpecification TargetSpecification{get { return _targetSpecification; }}


        public ValueRequirement(string valueName, ComputationTargetSpecification targetSpecification) : this(valueName, targetSpecification, ValueProperties.Create())
        {
        }

        public ValueRequirement(string valueName, ComputationTargetSpecification targetSpecification, ValueProperties constraints)
        {
            _valueName = valueName;
            _constraints = constraints;
            _targetSpecification = targetSpecification;
        }

        public bool IsSatisfiedBy(ValueSpecification valueSpecification) {
            if (ValueName != valueSpecification.ValueName) {
              return false;
            }
            if (! TargetSpecification.Equals(valueSpecification.TargetSpecification)) {
              return false;
            }
            if (!Constraints.IsSatisfiedBy(valueSpecification.Properties)) {
              return false;
            }
            return true;
        }




        public static ValueRequirement FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            ValueProperties constraints = deserializer.FromField<ValueProperties>(ffc.GetByName("constraints")) ?? ValueProperties.Create();

            var computationTargetType = ffc.GetValue<string>("computationTargetType");
            var computationTargetIdentifier = ffc.GetValue<string>("computationTargetIdentifier");
            var targetSpec = new ComputationTargetSpecification(EnumBuilder<ComputationTargetType>.Parse(computationTargetType), UniqueIdentifier.Parse(computationTargetIdentifier));
            var valueName = ffc.GetValue<string>("valueName");

            return new ValueRequirement(valueName, targetSpec, constraints);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer s)
        {
            msg.Add("valueName", ValueName);
            ComputationTargetSpecificationBuilder.AddMessageFields(s, msg, TargetSpecification);
            s.WriteInline(msg, "constraints", Constraints);
        }
    }
}