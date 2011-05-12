//-----------------------------------------------------------------------
// <copyright file="ComputedValue.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;

namespace OGDotNet.Mappedtypes.engine.Value
{
    public class ComputedValue
    {
        private readonly ValueSpecification _specification;
        private readonly object _value;

        public ComputedValue(ValueSpecification specification, object value)
        {
            _specification = specification;
            _value = value;
        }

        public ValueSpecification Specification
        {
            get { return _specification; }
        }

        public object Value
        {
            get { return _value; }
        }

        public static ComputedValue FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var valueSpecification = deserializer.FromField<ValueSpecification>(ffc.GetByName("specification"));
            var value = GetValue(deserializer, ffc.GetByName("value"), valueSpecification);
            return new ComputedValue(valueSpecification, value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField, ValueSpecification valueSpecification)
        {
            if (valueField.Type != FudgeMsgFieldType.Instance)
            {
                return valueField.Value;
            }

            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {//TODO I hope this gets a better type one day?
                return deserializer.FromField<List<double[]>>(valueField);
            }

            return deserializer.FromField(valueField, null);
        }
    }
}