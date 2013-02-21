// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetReference.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Engine.Target
{
    public abstract class ComputationTargetReference
    {
        private readonly ComputationTargetType _type;
        private readonly ComputationTargetReference _parent;

        protected ComputationTargetReference(ComputationTargetType type)
        {
            ArgumentChecker.NotNull(type, "type");
            _type = type;
        }

        protected ComputationTargetReference(ComputationTargetType type, ComputationTargetReference parent)
        {
            ArgumentChecker.NotNull(type, "type");
            ArgumentChecker.NotNull(parent, "parent");
            _type = type;
            _parent = parent;
        }

        public ComputationTargetType Type { get { return _type; }}

        public ComputationTargetReference Parent { get { return _parent; } }

        public abstract TResult Accept<TResult>(IComputationTargetReferenceVisitor<TResult> visitor);

        public abstract ComputationTargetSpecification Specification { get; }

        public ComputationTargetReference Containing(ComputationTargetType type, ExternalIdBundle identifiers)
        {
            return new ComputationTargetRequirement(type, identifiers, this);
        }

        public ComputationTargetReference Containing(ComputationTargetType type, UniqueId uniqueId)
        {
            return new ComputationTargetSpecification(type, uniqueId, this);
        }

        protected bool Equals(ComputationTargetReference other)
        {
            return Equals(_type, other._type) && Equals(_parent, other._parent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComputationTargetReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode() * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
            }
        }
    }
}