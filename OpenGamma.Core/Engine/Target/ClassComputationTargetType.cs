// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassComputationTargetType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Engine.Target
{
    public sealed class ClassComputationTargetType<TTarget> : ComputationTargetType
    {
        private readonly string _name;
        private readonly bool _nameWellKnown;

        public ClassComputationTargetType() : this(typeof(TTarget).Name, false)
        {
        }

        public ClassComputationTargetType(string name, bool nameWellKnown)
        {
            _name = name;
            _nameWellKnown = nameWellKnown;
        }

        public string Name { get { return _name; } }

        public bool IsNameWellKnown { get { return _nameWellKnown; } }

        public override string ToString()
        {
            return IsNameWellKnown ? Name : typeof(TTarget).FullName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ClassComputationTargetType<TTarget>;
        }

        public override int GetHashCode()
        {
            return typeof(TTarget).GetHashCode();
        }

        public override void Serialize(string fieldName, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            string serializedName;
            if (IsNameWellKnown)
            {
                serializedName = Name;
            }
            else
            {
                var typeMappingStrategy = (IFudgeTypeMappingStrategy)
                    serializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
                serializedName = typeMappingStrategy.GetName(typeof(TTarget));
            }
            msg.Add(fieldName, serializedName);
        }
    }
}