//-----------------------------------------------------------------------
// <copyright file="ValueAssertions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.cache;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.financial.model.volatility.surface;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.Timeseries.fast.longint;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Utils;
using Xunit;

namespace OGDotNet.Tests.Integration
{
    static class ValueAssertions
    {
        static readonly IEnumerable<MethodInfo> CandidateMethods = typeof(ValueAssertions).GetMethods().Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(object));

        private static readonly Memoizer<Type, MethodInfo> MethodCache = new Memoizer<Type, MethodInfo>(GetAppropriateMethod);
        private static MethodInfo GetAppropriateMethod(Type t)
        {
            var matchingMethods = CandidateMethods.Where(m =>
                    m.GetParameters()[0].ParameterType.IsAssignableFrom(t)
                    ||
                    (m.GetParameters()[0].ParameterType.IsGenericType && t.IsGenericType && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == t.GetGenericTypeDefinition()))
                .OrderByDescending(m => Preference(m.GetParameters()[0].ParameterType))
                .ToList();

            Assert.True(matchingMethods.Any(), string.Format("Don't know how to check value of type {0}", t));
            var appropriateMethod = matchingMethods.First();
            if (appropriateMethod.IsGenericMethod)
            {
                return appropriateMethod.MakeGenericMethod(t.GetGenericArguments());
            }
            else
            {
                return appropriateMethod;
            }
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
            var type = value.GetType();
            MethodCache.Get(type).Invoke(null, new[] {value});
        }

        public static void AssertSensibleValue<T1, T2>(Tuple<T1, T2> tuple)
        {
            AssertSensibleValue(tuple.Item1);
            AssertSensibleValue(tuple.Item2);
        }
        public static void AssertSensibleValue<TKey, TValue>(KeyValuePair<TKey, TValue> tuple)
        {
            AssertSensibleValue(tuple.Key);
            AssertSensibleValue(tuple.Value);
        }
        public static void AssertSensibleValue<TKey, TValue>(Pair<TKey, TValue> tuple) where TKey : class where TValue : class
        {
            AssertSensibleValue(tuple.First);
            AssertSensibleValue(tuple.Second);
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
            if (!value.Curve.IsVirtual)
            {
                var interestRate = value.GetInterestRate(value.Curve.XData[0]);
                AssertSensibleValue(interestRate);
                Assert.InRange(interestRate, -2, 2);
                var discountFactor = value.GetDiscountFactor(value.Curve.XData[0]);
                AssertSensibleValue(discountFactor);
            }
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

        public static void AssertSensibleValue(ISecurity security)
        {
            Assert.NotEmpty(security.Name);
            AssertSensibleValue(security.Identifiers);
            Assert.NotEmpty(security.SecurityType);
            Assert.NotNull(security.UniqueId);
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
                var d = value[x, y];
                AssertSensibleValue(d);
            }
        }

        public static void AssertSensibleValue(Tenor value)
        {
            Assert.NotNull(value);
            Assert.InRange(value.TimeSpan, TimeSpan.Zero, TimeSpan.MaxValue);
        }

        public static void AssertSensibleValue(ValueRequirement value)
        {
            Assert.NotNull(value);
            Assert.NotEmpty(value.ValueName);
        }

        public static void AssertSensibleValue(ValueSpecification spec)
        {
            Assert.NotNull(spec);
            Assert.NotEmpty(spec.ValueName);
        }

        public static void AssertSensibleValue(ICompiledViewDefinition viewDefin)
        {
            Assert.NotEmpty(viewDefin.LiveDataRequirements);
            AssertSensibleValue(viewDefin.LiveDataRequirements);

            AssertSensibleValue(viewDefin.Portfolio);
            Assert.NotNull(viewDefin.ViewDefinition);
            AssertSensibleValue(viewDefin.ViewDefinition.CalculationConfigurationsByName);

            Assert.Equal(default(DateTimeOffset) == viewDefin.EarliestValidity, viewDefin.LatestValidity == default(DateTimeOffset));
        }

        public static void AssertSensibleValue(ViewDefinition viewDefin)
        {
            Assert.NotNull(viewDefin.Name);
            Assert.NotNull(viewDefin.UniqueID);
        }
        public static void AssertSensibleValue(ViewCalculationConfiguration calculationConfiguration)
        {
            AssertSensibleValue(calculationConfiguration.SpecificRequirements);
            AssertSensibleValue(calculationConfiguration.PortfolioRequirementsBySecurityType);
        }

        public static void AssertSensibleValue(ValueProperties props)
        {
            Assert.NotNull(props);
        }

        public static void AssertSensibleValue(IPortfolio portfolio)
        {
            Assert.NotNull(portfolio);
            Assert.NotNull(portfolio.UniqueId);
            Assert.NotNull(portfolio.Name);
            AssertSensibleValue(portfolio.Root);
        }

        public static void AssertSensibleValue(PortfolioNode node)
        {
            Assert.NotNull(node);
            Assert.NotNull(node.UniqueId);
            Assert.NotNull(node.Name);
            Assert.NotNull(node.Positions);
            Assert.NotNull(node.SubNodes);
            AssertSensibleValue(node.Positions);
            AssertSensibleValue(node.SubNodes);
        }

        public static void AssertSensibleValue(IPosition position)
        {
            Assert.NotNull(position);
            Assert.NotNull(position.UniqueId);
            Assert.InRange(position.Quantity, -1, long.MaxValue); //Not sure why -1 is valid
            AssertSensibleValue(position.SecurityKey);
            Assert.NotNull(position.Trades);
            AssertSensibleValue(position.Trades);
        }

        public static void AssertSensibleValue(ITrade trade)
        {
            Assert.NotNull(trade);
            Assert.NotNull(trade.UniqueId);
            Assert.NotNull(trade.Counterparty);
            Assert.NotNull(trade.Counterparty.Identifier);
            Assert.NotEqual(default(DateTimeOffset), trade.TradeDate);
            AssertSensibleValue(trade.SecurityKey);
        }

        public static void AssertSensibleValue(IdentifierBundle bundle)
        {
            Assert.NotNull(bundle);
            Assert.NotEmpty(bundle.Identifiers);
            AssertSensibleValue(bundle.Identifiers);
        }

        public static void AssertSensibleValue(Identifier identifier)
        {
            Assert.NotNull(identifier);
        }

        public static void AssertSensibleValue(UniqueIdentifier identifier)
        {
            Assert.NotNull(identifier);
        }

        public static void AssertSensibleValue(SnapshotDataBundle bundle)
        {
            Assert.NotNull(bundle);
            AssertSensibleValue(bundle.DataPoints);
            Assert.NotEmpty(bundle.DataPoints);
        }

        public static void AssertSensibleValue(VolatilitySurface surface)
        {
            Assert.NotNull(surface);
            Assert.NotNull(surface.Sigma);
        }

        public static void AssertSensibleValue(MissingLiveDataSentinel sentinel)
        {
            Assert.Equal(MissingLiveDataSentinel.Instance, sentinel);
            Assert.True(ReferenceEquals(MissingLiveDataSentinel.Instance, sentinel));
        }

        public static void AssertSensibleValue(FastArrayLongDoubleTimeSeries series)
        {
            Assert.NotNull(series);
            Assert.NotEmpty(series.Values);
            AssertSensibleValue(series.Values);
        }

        public static void AssertSensibleValue(ILocalDateDoubleTimeSeries series)
        {
            Assert.NotNull(series);
            Assert.NotEmpty(series.Values);
            AssertSensibleValue(series.Values);
        }

        public static void AssertSensibleValue(DateTime dateTime)
        {
            var range = TimeSpan.FromDays(10000);
            Assert.InRange(dateTime, DateTime.Now - range, DateTime.Now + range);
        }
        public static void AssertSensibleValue(DateTimeOffset dateTime)
        {
            AssertSensibleValue(dateTime.ToUniversalTime().DateTime);
        }

        public static void AssertSensibleValue(VolatilityCubeDefinition defn)
        {
            Assert.NotNull(defn);
            AssertSensibleValue(defn.AllPoints);
        }
        public static void AssertSensibleValue(VolatilityPoint point)
        {
            Assert.NotNull(point);        
            AssertSensibleValue(point.OptionExpiry);
            AssertSensibleValue(point.SwapTenor);
            AssertSensibleValue(point.RelativeStrike);
        }
        public static void AssertSensibleValue(VolatilityCubeData data)
        {
            Assert.NotNull(data);
            AssertSensibleValue(data.DataPoints);
            AssertSensibleValue(data.Strikes);
            Assert.Empty(data.OtherData.DataPoints);
        }
    }
}
