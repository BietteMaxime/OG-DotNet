// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedComputationTargetType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Engine.Target
{
    public class NestedComputationTargetType : ComputationTargetType
    {
        private readonly IList<ComputationTargetType> _target;

        public NestedComputationTargetType(ComputationTargetType outerType, ComputationTargetType innerType)
        {
            var target = new List<ComputationTargetType>();
            AddTargets(outerType, target);
            AddTargets(innerType, target);
            _target = target;
        }

        private static void AddTargets(ComputationTargetType fromTarget, IList<ComputationTargetType> toTarget)
        {
            var nestedFromTarget = fromTarget as NestedComputationTargetType;
            if (nestedFromTarget == null)
            {
                if (fromTarget is NullComputationTargetType)
                {
                    throw new InvalidOperationException();
                }
                toTarget.Add(fromTarget);
            }
            else
            {
                foreach (ComputationTargetType constituent in nestedFromTarget.Target)
                {
                    toTarget.Add(constituent);
                }
            }
        }

        public IList<ComputationTargetType> Target { get { return _target; }}

        protected bool Equals(NestedComputationTargetType other)
        {
            return Equals(_target, other._target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NestedComputationTargetType) obj);
        }

        public override int GetHashCode()
        {
            return typeof(NestedComputationTargetType).GetHashCode() * 31 + _target.GetHashCode();
        }

        public override void Serialize(string fieldName, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            foreach (ComputationTargetType constituent in Target)
            {
                constituent.Serialize(fieldName, msg, serializer);
            }
        }
    }
}