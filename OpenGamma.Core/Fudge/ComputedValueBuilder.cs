// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputedValueBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Util.Tuple;

namespace OpenGamma.Fudge
{
    class ComputedValueBuilder : BuilderBase<ComputedValue>
    {
        public ComputedValueBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ComputedValue DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var valueSpecification = deserializer.FromField<ValueSpecification>(msg.GetByName("specification"));
            var value = GetValue(deserializer, msg.GetByName("value"), valueSpecification);
            return new ComputedValue(valueSpecification, value);
        }

        public static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField, ValueSpecification valueSpecification)
        {
            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {
                var fudgeFieldContainer = (IFudgeFieldContainer)valueField.Value;

                // TODO I hope this gets a better type one day?
                return fudgeFieldContainer.Where(f => !f.Ordinal.HasValue).Select(f => (double[])f.Value).ToList();
            }

            return Pair.FromField(deserializer, valueField);
        }
    }
}
