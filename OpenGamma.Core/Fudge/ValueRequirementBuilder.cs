// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueRequirementBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Engine.Target;
using OpenGamma.Engine.Value;

namespace OpenGamma.Fudge
{
    internal class ValueRequirementBuilder : BuilderBase<ValueRequirement>
    {
        private const string ValueNameField = "valueName";
        private const string ConstraintsField = "constraints";

        public ValueRequirementBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueRequirement obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add(ValueNameField, obj.ValueName);
            ComputationTargetReferenceBuilder.SerializeCore(obj.TargetReference, msg, serializer);
            serializer.WriteInline(msg, ConstraintsField, obj.Constraints);
        }

        protected override ValueRequirement DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var valueName = msg.GetString(ValueNameField);
            ComputationTargetReference targetReference = ComputationTargetReferenceBuilder.DeserializeCore(msg, deserializer);

            IFudgeField constraintsField = msg.GetByName(ConstraintsField);
            if (constraintsField == null)
            {
                return new ValueRequirement(valueName, targetReference);
            }
            return new ValueRequirement(valueName, targetReference, deserializer.FromField<ValueProperties>(constraintsField));
        }
    }
}