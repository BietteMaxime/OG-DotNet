//-----------------------------------------------------------------------
// <copyright file="ComputedValueBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Util.Tuple;

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
            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {
                var fudgeFieldContainer = (IFudgeFieldContainer)valueField.Value;
                //TODO I hope this gets a better type one day?
                return fudgeFieldContainer.Where(f => !f.Ordinal.HasValue).Select(f => (double[])f.Value).ToList();
            }

            return Pair.FromField(deserializer, valueField);
        }
    }
}
