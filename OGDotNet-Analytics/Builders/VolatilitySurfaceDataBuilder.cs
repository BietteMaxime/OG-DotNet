//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceDataBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Builders
{
    class VolatilitySurfaceDataBuilder : BuilderBase<VolatilitySurfaceData>
    {
        public VolatilitySurfaceDataBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override VolatilitySurfaceData DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            Currency currency = ffc.GetValue<Currency>("currency");
            string definitionName = ffc.GetValue<string>("definitionName");
            string specificationName = ffc.GetValue<string>("specificationName");
            string interpolatorName = ffc.GetValue<string>("interpolatorName");

            IList<object> xs = ffc.GetAllByName("xs").Select(deserializer.FromField<object>).ToList();
            IList<object> ys = ffc.GetAllByName("ys").Select(deserializer.FromField<object>).ToList();

            Type xType = GetType(xs);
            Type yType = GetType(ys);
            var values = new Dictionary<Tuple<object, object>, double>();
            var valuesFields = ffc.GetAllByName("values");
            foreach (var valueField in valuesFields)
            {
                var subMessage = (IFudgeFieldContainer)valueField.Value;
                object x = deserializer.FromField(subMessage.GetByName("x"), xType);
                object y = deserializer.FromField(subMessage.GetByName("y"), yType);
                double value = subMessage.GetValue<double>("value");
                values.Add(new Tuple<object, object>(x, y), value);
            }

            return Build(xType, yType, definitionName, specificationName, currency, interpolatorName, xs, ys, values);
        }

        static readonly MethodInfo GenericBuildMethod = typeof(VolatilitySurfaceDataBuilder).GetMethods().Where(m => m.Name == "Build" && m.IsGenericMethodDefinition).Single();

        private VolatilitySurfaceData Build(Type xType, Type yType, string definitionName, string specificationName, Currency currency, string interpolatorName, IList<object> xs, IList<object> ys, Dictionary<Tuple<object, object>, double> values)
        {
            return (VolatilitySurfaceData) GenericBuildMethod.MakeGenericMethod(new[] { xType, yType }).Invoke(this, new object[] { definitionName, specificationName, currency, interpolatorName, xs, ys, values });
        }

        public VolatilitySurfaceData<TX, TY> Build<TX, TY>(string definitionName, string specificationName, Currency currency, string interpolatorName, IList<object> xs, IList<object> ys, Dictionary<Tuple<object, object>, double> values)
        {
            return new VolatilitySurfaceData<TX, TY>(definitionName, specificationName, currency, interpolatorName, xs.Cast<TX>().ToList(), ys.Cast<TY>().ToList(),
                values.ToDictionary(kvp => Tuple.Create((TX)kvp.Key.Item1, (TY)kvp.Key.Item2), kvp => kvp.Value)
                );
        }

        private static Type GetType(IEnumerable<object> os)
        {
            if (! os.Any())
            {
                throw new ArgumentException();
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
