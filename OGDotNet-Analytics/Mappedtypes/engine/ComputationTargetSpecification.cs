//-----------------------------------------------------------------------
// <copyright file="ComputationTargetSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Engine
{
    [FudgeSurrogate(typeof(ComputationTargetSpecificationBuilder))]
    public class ComputationTargetSpecification : IEquatable<ComputationTargetSpecification>
    {
        private readonly ComputationTargetType _type;
        private readonly UniqueId _uid;

        public ComputationTargetSpecification(ComputationTargetType type, UniqueId uid)
        {
            _type = type;
            _uid = uid;
        }

        public ComputationTargetType Type
        {
            get { return _type; }
        }

        public UniqueId Uid
        {
            get { return _uid; }
        }

        //NOTE: non standard equals method, since ComputationTargetType is an enum

        public bool Equals(ComputationTargetSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._type == _type && Equals(other._uid, _uid);
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
                return ((int)_type * 397) ^ (_uid != null ? _uid.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("[ComputationTargetSpecification {0} {1}]", Uid, Type);
        }
    }
}