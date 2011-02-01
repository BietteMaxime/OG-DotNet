﻿using Fudge.Serialization;
using OGDotNet_Analytics.Builders;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Mappedtypes.engine
{
    [FudgeSurrogate(typeof(ComputationTargetSpecificationBuilder))]
    public class ComputationTargetSpecification
    {
        private readonly ComputationTargetType _type;
        private readonly UniqueIdentifier _uid;

        public ComputationTargetSpecification(ComputationTargetType type, UniqueIdentifier uid)
        {
            _type = type;
            _uid = uid;
        }

        public ComputationTargetType Type
        {
            get { return _type; }
        }

        public UniqueIdentifier Uid
        {
            get { return _uid; }
        }

        public bool Equals(ComputationTargetSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._type, _type) && Equals(other._uid, _uid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ComputationTargetSpecification)) return false;
            return Equals((ComputationTargetSpecification)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode() * 397) ^ (_uid != null ? _uid.GetHashCode() : 0);
            }
        }
    }
}