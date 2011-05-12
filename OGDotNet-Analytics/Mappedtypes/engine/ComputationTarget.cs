//-----------------------------------------------------------------------
// <copyright file="ComputationTarget.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine
{
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

        public UniqueIdentifier UniqueId
        {
            get
            {
                var uniqueIdentifiable = Value as IUniqueIdentifiable;
                return uniqueIdentifiable == null ? null : uniqueIdentifiable.UniqueId;
            }
        }

        public static ComputationTarget FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
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
                    return UniqueIdentifier.Parse((string) valueField.Value);
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
