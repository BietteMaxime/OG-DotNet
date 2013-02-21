// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTarget.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;
using OpenGamma.Engine.Target;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Engine
{
    [FudgeSurrogate(typeof(ComputationTargetBuilder))]
    public class ComputationTarget
    {
        private readonly ComputationTargetType _type;
        private readonly object _value;

        public ComputationTarget(ComputationTargetType type, object value)
        {
            _type = type;
            _value = value;
        }

        public ComputationTargetType Type
        {
            get { return _type; }
        }

        public object Value
        {
            get { return _value; }
        }

        public UniqueId UniqueId
        {
            get
            {
                var uniqueIdentifiable = Value as IUniqueIdentifiable;
                return uniqueIdentifiable == null ? null : uniqueIdentifiable.UniqueId;
            }
        }
    }
}
