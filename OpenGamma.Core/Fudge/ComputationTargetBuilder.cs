// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Engine;
using OpenGamma.Engine.Target;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    class ComputationTargetBuilder : BuilderBase<ComputationTarget>
    {
        public ComputationTargetBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ComputationTarget DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            ComputationTargetType computationTargetType = ComputationTargetTypeBuilder.DeserializeCore(msg, deserializer);
            object value = GetValue(deserializer, msg.GetByName("value"));
            return new ComputationTarget(computationTargetType, value);
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField)
        {
            if (!Equals(valueField.Type, FudgeMsgFieldType.Instance))
            {
                if (valueField.Value is string)
                {
                    return UniqueId.Parse((string)valueField.Value);
                }

                throw new ArgumentException("Computation target type which I don't know how to deserialize");
            }

            return deserializer.FromField<object>(valueField);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
