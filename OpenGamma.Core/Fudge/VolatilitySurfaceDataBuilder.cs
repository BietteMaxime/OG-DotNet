// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VolatilitySurfaceDataBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Fudge;
using Fudge.Serialization;

using OpenGamma.MarketDataSnapshot;
using OpenGamma.Util;
using OpenGamma.Util.Money;

namespace OpenGamma.Fudge
{
    class VolatilitySurfaceDataBuilder : BuilderBase<VolatilitySurfaceData>
    {
        public VolatilitySurfaceDataBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override VolatilitySurfaceData DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var currency = msg.GetValue<Currency>("currency");
            var definitionName = msg.GetValue<string>("definitionName");
            var specificationName = msg.GetValue<string>("specificationName");
            var interpolatorName = msg.GetValue<string>("interpolatorName");

            bool xWrapped;
            IList<object> xs = ReadAllAsObjectList(msg, "xs", deserializer, out xWrapped);
            bool yWrapped;
            IList<object> ys = ReadAllAsObjectList(msg, "ys", deserializer, out yWrapped);

            Type xType = GetType(xs);
            Type yType = GetType(ys);
            var values = new Dictionary<Tuple<object, object>, double>();
            var valuesFields = msg.GetAllByName("values");
            foreach (var valueField in valuesFields)
            {
                var subMessage = (IFudgeFieldContainer)valueField.Value;
                var xField = subMessage.GetByName("x");
                var yField = subMessage.GetByName("y");
                object x = xWrapped ? GetWrappedPrimitive(xField) : deserializer.FromField(xField, xType);
                object y = yWrapped ? GetWrappedPrimitive(yField) : deserializer.FromField(yField, yType);
                var value = subMessage.GetValue<double>("value");
                values.Add(new Tuple<object, object>(x, y), value);
            }

            return Build(xType, yType, definitionName, specificationName, currency, interpolatorName, xs, ys, values);
        }

        private static IList<object> ReadAllAsObjectList(IFudgeFieldContainer ffc, string name, IFudgeDeserializer deserializer, out bool wrappedPrimitive)
        {
            var fields = ffc.GetAllByName(name);
            if (IsWrappedPrimitive(fields[0]))
            {
                wrappedPrimitive = true;
                return fields.Select(GetWrappedPrimitive).ToList();    
            }

            wrappedPrimitive = false;
            return fields.Select(deserializer.FromField<object>).ToList();
        }

        private static object GetWrappedPrimitive(IFudgeField fudgeField)
        {
            return ((IFudgeFieldContainer) fudgeField.Value).GetByName("value").Value;
        }

        private static bool IsWrappedPrimitive(IFudgeField fudgeField)
        {
            var ffc = fudgeField.Value as IFudgeFieldContainer;
            if (ffc == null)
            {
                return false;
            }

            var valueFields = ffc.GetAllByName("value");
            var valueFieldCount = valueFields.Count;
            if (valueFieldCount != 1)
            {
                return false;
            }

            var allByOrdinal = ffc.GetAllByOrdinal(0);
            if (allByOrdinal.Count == 0)
            {
                return false;
            }

            if (valueFieldCount + allByOrdinal.Count != ffc.Count())
            {
                return false;
            }

            if (valueFieldCount != 1)
            {
                return false;
            }

            if (valueFields.Single().Value is IFudgeFieldContainer)
            {
                return false;
            }

            return true;
        }

        static readonly MethodInfo GenericBuildMethod = GenericUtils.GetGenericMethod(typeof(VolatilitySurfaceDataBuilder), "Build");

        private static VolatilitySurfaceData Build(Type xType, Type yType, string definitionName, string specificationName, Currency currency, string interpolatorName, IList<object> xs, IList<object> ys, Dictionary<Tuple<object, object>, double> values)
        {
            return (VolatilitySurfaceData) GenericUtils.Call(GenericBuildMethod, new[] { xType, yType }, new object[] { definitionName, specificationName, currency, interpolatorName, xs, ys, values });
        }

        public static VolatilitySurfaceData<TX, TY> Build<TX, TY>(string definitionName, string specificationName, Currency currency, string interpolatorName, IList<object> xs, IList<object> ys, Dictionary<Tuple<object, object>, double> values)
        {
            return new VolatilitySurfaceData<TX, TY>(definitionName, specificationName, currency, interpolatorName, xs.Cast<TX>().ToList(), ys.Cast<TY>().ToList(), 
                values.ToDictionary(kvp => Tuple.Create((TX)kvp.Key.Item1, (TY)kvp.Key.Item2), kvp => kvp.Value)
                );
        }

        private static Type GetType(IEnumerable<object> os)
        {
            if (!os.Any())
            {
                return typeof(object);
            }

            Type tSeed = os.First().GetType();
            return os.Aggregate(tSeed, (t, o) => GetParent(t, o.GetType()));
        }

        private static Type GetParent(Type type, Type getType)
        {
            if (type.IsAssignableFrom(getType))
            {
                return type;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
