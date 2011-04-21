//-----------------------------------------------------------------------
// <copyright file="ViewCalculationResultModelBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.View;

namespace OGDotNet.Builders
{
    internal class ViewCalculationResultModelBuilder : BuilderBase<ViewCalculationResultModel>
    {
        public ViewCalculationResultModelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }
        public override ViewCalculationResultModel DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var map = new Dictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>>();

            foreach (var field in msg)
            {
                var subMsg = (IFudgeFieldContainer) field.Value;

                ValueSpecification valueSpecification = null;
                object innerValue = null;

                foreach (var subField in subMsg)
                {
                    switch (subField.Name)
                    {
                        case "specification":
                            valueSpecification = deserializer.FromField<ValueSpecification>(subField);
                            break;
                        case "value":
                            innerValue = GetValue(deserializer, subField, valueSpecification);
                            break;
                        default:
                            break;
                    }
                }

                var value = new ComputedValue(valueSpecification, innerValue);
                
                ComputationTargetSpecification target = value.Specification.TargetSpecification;
                
                if (!map.ContainsKey(target)) {
                    map.Add(target, new Dictionary<string, ComputedValue>());
                }
                map[target].Add(value.Specification.ValueName, value);
            }
            return new ViewCalculationResultModel(map);
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeField valueField, ValueSpecification valueSpecification)
        {
            if (valueField.Type != FudgeMsgFieldType.Instance)
            {
                return valueField.Value;
            }

            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {
                var fromField = deserializer.FromField<List<double[]>>(valueField);
                return fromField; //TODO I hope this gets a better type one day?
            }

            return deserializer.FromField(valueField, null);
        }
    }
}