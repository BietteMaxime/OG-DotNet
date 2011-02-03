using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.Value
{
    public class ValueSpecification : IEquatable<ValueSpecification>
    {
        private readonly string _valueName;
        private readonly ComputationTargetSpecification _targetSpecification;

        public ValueSpecification(string valueName, ComputationTargetSpecification targetSpecification)
        {
            _valueName = valueName;
            _targetSpecification = targetSpecification;
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public ComputationTargetSpecification TargetSpecification
        {
            get { return _targetSpecification; }
        }


        public static ValueSpecification FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var valueName = ffc.GetValue<String>("valueName");
            var targetSpecification = new ComputationTargetSpecificationBuilder(deserializer.Context, typeof(ComputationTargetSpecification)).DeserializeImpl(ffc, deserializer); //Can't register twice
            //TODO properties
            return new ValueSpecification(valueName, targetSpecification);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ValueSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._valueName, _valueName) && Equals(other._targetSpecification, _targetSpecification);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ValueSpecification)) return false;
            return Equals((ValueSpecification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_valueName != null ? _valueName.GetHashCode() : 0)*397) ^ (_targetSpecification != null ? _targetSpecification.GetHashCode() : 0);
            }
        }
    }
}