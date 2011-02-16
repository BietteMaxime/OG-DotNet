using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Utils;
using Xunit;

namespace OGDotNet.Tests.Integration
{
    static class ValueAssertions
    {
        public static readonly Memoizer<Type,MethodInfo> MethodCache = new Memoizer<Type, MethodInfo>(GetAppropriateMethod);
        private static MethodInfo GetAppropriateMethod(Type t)
        {
            var enumerable = typeof(ValueAssertions)
                .GetMethods()
                .Where(m => m.ReturnType == typeof(void) 
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType!= typeof(object)
                    && m.GetParameters()[0].ParameterType.IsAssignableFrom(t)
                    )
                .OrderByDescending(m => Preference(m.GetParameters()[0].ParameterType))
                .ToList();
            Assert.True(enumerable.Count>0, string.Format("Don't know how to check value of type {0}", t));
            return enumerable.First();
        }

        private static object Preference(Type parameterType)
        {
            if (parameterType.IsInterface)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public static void AssertSensibleValue(object value)
        {
            Assert.NotNull(value);
            MethodCache.Get(value.GetType()).Invoke(null, new[] {value});
        }

        public static void AssertSensibleValue(IEnumerable values)
        {
            Assert.NotNull(values);
            foreach (var value in values)
            {
                AssertSensibleValue(value);
            }
        }

        public static void AssertSensibleValue(string value)
        {
            Assert.NotNull(value);
        }

        public static void AssertSensibleValue(LabelledMatrixEntry value)
        {
            AssertSensibleValue(value.Label);
            AssertSensibleValue(value.Value);
        }

        public static void AssertSensibleValue(double value)
        {
            //Thsi is valid apparently Assert.False(Double.IsNaN(value));
            Assert.False(Double.IsInfinity(value));
        }

        public static void AssertSensibleValue(YieldCurve value)
        {
            AssertSensibleValue(value.Curve);
            var interestRate = value.GetInterestRate(value.Curve.XData[0]);
            AssertSensibleValue(interestRate);
            var discountFactor = value.GetDiscountFactor(value.Curve.XData[0]);
            AssertSensibleValue(discountFactor);
            Assert.InRange(discountFactor, 0, 1);
        }
        public static void AssertSensibleValue(Curve value)
        {
            Assert.NotNull(value);
            Assert.NotNull(value.Name);
            if (value.IsVirtual)
            {
                Assert.Throws<InvalidOperationException>(() => value.XData);
                Assert.Throws<InvalidOperationException>(() => value.YData);
            }
            else
            {
                Assert.Equal(value.XData.Count, value.YData.Count);
                Assert.Equal(value.XData.Count, value.GetData().Count());
            }
        }

        public static void AssertSensibleValue(VolatilitySurfaceData value)
        {
            Assert.NotNull(value);

            AssertSensibleValue(value.Xs); 
            AssertSensibleValue(value.Ys);

            AssertSensibleValue(value.Xs.Select(value.GetXSlice)); 
            AssertSensibleValue(value.Ys.Select(value.GetYSlice));

            var allPoints = value.Xs.Join(value.Ys, x => 1, y => 1, (x, y) => new Tuple<Tenor, Tenor>(x, y)).ToList();
            foreach (var xy in allPoints)
            {
                var x = xy.Item1;
                var y = xy.Item2;
                var d = value[x,y];
                AssertSensibleValue(d);
            }
        }

        public static void AssertSensibleValue(Tenor value)
        {
            Assert.NotNull(value);
            Assert.InRange(value.TimeSpan, TimeSpan.Zero, TimeSpan.MaxValue);
        }
    }
}
