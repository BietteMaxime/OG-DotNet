// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.Target;
using OpenGamma.Id;

namespace OpenGamma.Engine
{
    public class ComputationTargetSpecification : ComputationTargetReference
    {
        public static readonly ComputationTargetSpecification Null = new ComputationTargetSpecification(ComputationTargetType.Null, null);

        private readonly UniqueId _uniqueId;

        public ComputationTargetSpecification(ComputationTargetType type, UniqueId uniqueId) : base(type)
        {
            _uniqueId = uniqueId;
        }

        public ComputationTargetSpecification(ComputationTargetType type, UniqueId uniqueId, ComputationTargetReference parent) : base(type, parent)
        {
            _uniqueId = uniqueId;
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public override TResult Accept<TResult>(IComputationTargetReferenceVisitor<TResult> visitor)
        {
            return visitor.VisitComputationTargetSpecification(this);
        }

        public override ComputationTargetSpecification Specification
        {
            get { return this; }
        }

        protected bool Equals(ComputationTargetSpecification other)
        {
            return base.Equals(other) && Equals(_uniqueId, other._uniqueId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComputationTargetSpecification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_uniqueId != null ? _uniqueId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("CTSpec[{0}, {1}]", Type, UniqueId);
        }
    }
}