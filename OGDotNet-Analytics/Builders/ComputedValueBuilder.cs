//-----------------------------------------------------------------------
// <copyright file="ComputedValueBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.Engine.value;

namespace OGDotNet.Builders
{
    class ComputedValueBuilder : BuilderBase<ComputedValue>
    {
        public ComputedValueBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ComputedValue DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var valueSpecification = deserializer.FromField<ValueSpecification>(ffc.GetByName("specification"));
            var value = GetValue(deserializer, ffc.GetByName("value"), valueSpecification);
            return new ComputedValue(valueSpecification, value);
        }

        public static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField, ValueSpecification valueSpecification)
        {
            if (valueField.Type != FudgeMsgFieldType.Instance)
            {
                return valueField.Value;
            }

            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {//TODO I hope this gets a better type one day?
                return
                    ((IFudgeFieldContainer)valueField.Value).Where(f => !f.Ordinal.HasValue).Select(f => (double[])f.Value).ToList();
            }

            return deserializer.FromField(valueField, null);
        }
    }
}
