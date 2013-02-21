// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleComputationTargetType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Engine.Target
{
    public class MultipleComputationTargetType : ComputationTargetType
    {
        private readonly ISet<ComputationTargetType> _target;

        public MultipleComputationTargetType(ComputationTargetType a, ComputationTargetType b)
        {
            ISet<ComputationTargetType> target = new HashSet<ComputationTargetType>();
            AddTargets(a, target);
            AddTargets(b, target);
            _target = target;
        }

        private static void AddTargets(ComputationTargetType fromTarget, ISet<ComputationTargetType> toTarget)
        {
            var multipleFromTarget = fromTarget as MultipleComputationTargetType;
            if (multipleFromTarget == null)
            {
                toTarget.Add(fromTarget);
            }
            else
            {
                foreach (ComputationTargetType constituent in multipleFromTarget.Target)
                {
                    toTarget.Add(constituent);
                }
            }
        }

        public ISet<ComputationTargetType> Target { get { return _target; } }

        protected bool Equals(MultipleComputationTargetType other)
        {
            return Equals(_target, other._target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MultipleComputationTargetType) obj);
        }

        public override int GetHashCode()
        {
            return typeof(MultipleComputationTargetType).GetHashCode() * 31 + _target.GetHashCode();
        }

        public override void Serialize(string fieldName, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            var multipleMsg = serializer.Context.NewMessage();
            foreach (ComputationTargetType constituent in Target)
            {
                if (constituent is NestedComputationTargetType)
                {
                    FudgeMsg nestedMsg = serializer.Context.NewMessage();
                    constituent.Serialize(null, nestedMsg, serializer);
                    multipleMsg.Add(null, null, nestedMsg);
                }
                else
                {
                    constituent.Serialize(null, multipleMsg, serializer);
                }
            }
            msg.Add(fieldName, multipleMsg);
        }
    }
}