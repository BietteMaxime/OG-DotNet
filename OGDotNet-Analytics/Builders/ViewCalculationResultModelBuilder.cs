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
                
                var valueSpecification = deserializer.FromField<ValueSpecification>(subMsg.GetByName("specification"));
                object innerValue = GetValue(deserializer, subMsg, valueSpecification);

                var value = new ComputedValue(valueSpecification, innerValue);
                
                ComputationTargetSpecification target = value.Specification.TargetSpecification;
                if (!map.ContainsKey(target)) {
                    map.Add(target, new Dictionary<String, ComputedValue>());
                }
                map[target].Add(value.Specification.ValueName, value);
            }
            return new ViewCalculationResultModel(map);
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeFieldContainer subMsg, ValueSpecification valueSpecification)
        {
            var o = subMsg.GetByName("value");
            object innerValue;

            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {
                var fromField = deserializer.FromField<List<double[]>>(o);
                return fromField;//TODO I hope this gets a better type one day?
            }

            
            var t = o.Type.CSharpType;
            if (o.Type == FudgeMsgFieldType.Instance || o.Type == IndicatorFieldType.Instance)
            {
                try
                {
                    innerValue = deserializer.FromField(o, t);
                }
                catch (ArgumentNullException)
                {//TODO: didn't know how to deserialize
                    innerValue = new UnhandledObject(o.Value);
                }
            }
            else
            {
                innerValue = subMsg.GetValue("value");
            }
            
            return innerValue;
        }

        private class UnhandledObject
        {
            //Useful for debugging
            private readonly object _value;

            public UnhandledObject(object value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return "An Object I didn't know how to deserialize";
            }
        }
    }
}