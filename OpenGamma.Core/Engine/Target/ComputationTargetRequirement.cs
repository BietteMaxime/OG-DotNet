// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetRequirement.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Engine.Target
{
    public class ComputationTargetRequirement : ComputationTargetReference
    {
        private readonly ExternalIdBundle _identifiers;

        public ComputationTargetRequirement(ComputationTargetType type, ExternalId id) : base(type)
        {
            if (type is NullComputationTargetType)
            {
                ArgumentChecker.True(id == null, "id");
                _identifiers = ExternalIdBundle.Empty();
            }
            else
            {
                ArgumentChecker.NotNull(id, "id");
                _identifiers = ExternalIdBundle.Create(id);
            }
        }

        public ComputationTargetRequirement(ComputationTargetType type, ExternalIdBundle identifiers)
            : base(type)
        {
            if (type is NullComputationTargetType)
            {
                ArgumentChecker.True(identifiers == null || identifiers.IsEmpty(), "identifiers");
                _identifiers = ExternalIdBundle.Empty();
            }
            else
            {
                ArgumentChecker.NotNull(identifiers, "identifiers");
                ArgumentChecker.False(identifiers.IsEmpty(), "identifiers");
                _identifiers = identifiers;
            }
        }

        public ComputationTargetRequirement(ComputationTargetType type, ExternalIdBundle identifiers, ComputationTargetReference parent) : base(type, parent)
        {
            ArgumentChecker.NotNull(identifiers, "identifiers");
            ArgumentChecker.False(identifiers.IsEmpty(), "identifiers");
            _identifiers = identifiers;
        }

        public ExternalIdBundle Identifiers { get { return _identifiers; } }

        public override TResult Accept<TResult>(IComputationTargetReferenceVisitor<TResult> visitor)
        {
            return visitor.VisitComputationTargetRequirement(this);
        }

        public override ComputationTargetSpecification Specification
        {
            get { throw new OpenGammaException(string.Format("{0} is not resolved", this)); }
        }

        protected bool Equals(ComputationTargetRequirement other)
        {
            return base.Equals(other) && Equals(_identifiers, other._identifiers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComputationTargetRequirement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ _identifiers.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("CTReq[{0}, {1}]", Type, Identifiers);
        }
    }
}