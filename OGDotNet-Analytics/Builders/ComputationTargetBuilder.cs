//-----------------------------------------------------------------------
// <copyright file="ComputationTargetBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class ComputationTargetBuilder : BuilderBase<ComputationTarget>
    {
        public ComputationTargetBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ComputationTarget DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var computationTargetType = EnumBuilder<ComputationTargetType>.Parse(ffc.GetMessage("type").GetString(1));
            object value = GetValue(deserializer, ffc.GetByName("value"));
            return new ComputationTarget(computationTargetType, value);
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField)
        {
            if (valueField.Type != FudgeMsgFieldType.Instance)
            {
                if (valueField.Value is string)
                {
                    return UniqueIdentifier.Parse((string)valueField.Value);
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
