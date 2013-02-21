// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullComputationTargetType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;
using OpenGamma.Fudge;

namespace OpenGamma.Engine.Target
{
    [FudgeSurrogate(typeof(ComputationTargetTypeBuilder))]
    public sealed class NullComputationTargetType : ComputationTargetType
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is NullComputationTargetType;
        }

        public override int GetHashCode()
        {
            return typeof(NullComputationTargetType).GetHashCode();
        }

        public override string ToString()
        {
            return "NULL";
        }

        public override void Serialize(string fieldName, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add(fieldName, "NULL");
        }
    }
}