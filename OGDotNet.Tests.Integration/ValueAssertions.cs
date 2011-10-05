﻿//-----------------------------------------------------------------------
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
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Engine.Function.Resolver;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Cache;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Financial.Analytics;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Cube;
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.SABR;
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.Financial.InterestRate;
using OGDotNet.Mappedtypes.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Financial.Model.Volatility.Surface;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.JavaX.Time.Calendar;
using OGDotNet.Mappedtypes.Math.Curve;
using OGDotNet.Mappedtypes.Math.Matrix;
using OGDotNet.Mappedtypes.Math.Surface;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.Timeseries.Fast.LongInt;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Utils;
using Xunit;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

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
                    (m.IsGenericMethodDefinition && m.GetParameters()[0].ParameterType.IsGenericType && t.IsGenericType && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == t.GetGenericTypeDefinition()))
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
       
        public static void AssertSensibleValue<TKey, TValue>(Pair<TKey, TValue> tuple)
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

        public static void AssertSensibleValue(Currency currency)
        {
            Assert.NotNull(currency);
        }

        public static void AssertSensibleValue(LabelledMatrixEntry value)
        {
            AssertSensibleValue(value.Label);
            AssertSensibleValue(value.Value);
        }

        public static void AssertSensibleValue(DoubleLabelledMatrix2D dlm)
        {
            Assert.NotNull(dlm);
            foreach (var entry in dlm)
            {
                Assert.Contains(entry.XLabel, dlm.XLabels);
                Assert.Contains(entry.YLabel, dlm.YLabels);
            }
            //other checks are done in type
        }

        public static void AssertSensibleValue(long value)
        {
            Assert.NotEqual(long.MaxValue, value);
            Assert.NotEqual(long.MinValue, value);
        }

        public static void AssertSensibleValue(sbyte value)
        {
            Assert.NotEqual(sbyte.MaxValue, value);
            Assert.NotEqual(sbyte.MinValue, value);
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

        public static void AssertSensibleValue(VolatilitySurfaceData<Tenor, Tenor> value)
        {
            AssertSensibleValue<Tenor, Tenor>(value);
            bool anyXSlice = false;
            foreach (var x in value.Xs)
            {
                try
                {
                    AssertSensibleValue(value.GetXSlice(x));
                    anyXSlice = true;
                }
                catch (KeyNotFoundException)
                {
                }
            }
            Assert.True(anyXSlice, "No x slices could be got");

            bool anyYSlice = false;
            foreach (var y in value.Ys)
            {
                try
                {
                    AssertSensibleValue(value.GetYSlice(y));
                    anyYSlice = true;
                }
                catch (KeyNotFoundException)
                {
                }
            }
            Assert.True(anyYSlice, "No y slices could be got");
        }

        public static void AssertSensibleValue<TX, TY>(VolatilitySurfaceData<TX, TY> value)
        {
            Assert.NotNull(value);

            AssertSensibleValue(value.Xs);
            AssertSensibleValue(value.Ys);

            var allPoints = value.Xs.Join(value.Ys, x => 1, y => 1, (x, y) => new Tuple<TX, TY>(x, y)).ToList();
            foreach (var xy in allPoints)
            {
                var x = xy.Item1;
                var y = xy.Item2;
                double d;
                if (value.TryGet(x, y, out d))
                {
                    AssertSensibleValue(d);    
                }
            }
        }

        public static void AssertSensibleValue(BloombergFXOptionVolatilitySurfaceInstrumentProvider.FXVolQuoteType type)
        {
            Assert.NotNull(type);
            Assert.True(EnumUtils.EnumValues<BloombergFXOptionVolatilitySurfaceInstrumentProvider.FXVolQuoteType>().Contains(type));
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
            Assert.NotEmpty(viewDefin.MarketDataRequirements);
            AssertSensibleValue(viewDefin.MarketDataRequirements);

            if (viewDefin.Portfolio != null)
            {
                AssertSensibleValue(viewDefin.Portfolio);
            }
            Assert.NotNull(viewDefin.ViewDefinition);

            AssertSensibleValue(viewDefin.ViewDefinition.CalculationConfigurationsByName);
            foreach (var kvp in viewDefin.ViewDefinition.CalculationConfigurationsByName)
            {
                Assert.Equal(kvp.Key, kvp.Value.Name);
            }

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
            AssertSensibleValue(calculationConfiguration.ResolutionRuleTransform);
        }

        public static void AssertSensibleValue(IResolutionRuleTransform transform)
        {
            Assert.NotNull(transform);
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
            Assert.InRange(position.Quantity, -20, decimal.MaxValue); //Not sure why -20 is valid (Bond Future Test View)
            AssertSensibleValue(position.SecurityKey);
            Assert.NotNull(position.Trades);
            AssertSensibleValue(position.Trades);
        }

        public static void AssertSensibleValue(ITrade trade)
        {
            Assert.NotNull(trade);
            Assert.NotNull(trade.UniqueId);
            Assert.NotNull(trade.Counterparty);
            Assert.NotNull(trade.Counterparty.ExternalId);
            Assert.NotEqual(default(DateTimeOffset), trade.TradeDate);
            AssertSensibleValue(trade.SecurityKey);
        }

        public static void AssertSensibleValue(ExternalIdBundle bundle)
        {
            Assert.NotNull(bundle);
            Assert.NotEmpty(bundle.Identifiers);
            AssertSensibleValue(bundle.Identifiers);
        }

        public static void AssertSensibleValue(ExternalId identifier)
        {
            Assert.NotNull(identifier);
        }

        public static void AssertSensibleValue(UniqueId identifier)
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
            AssertSensibleValue(surface.Sigma);
        }

        public static void AssertSensibleValue(ConstantDoublesSurface surface)
        {
            var zValue = surface.GetZValue(23, 23);
            AssertSensibleValue(zValue);
        }

        public static void AssertSensibleValue(InterpolatedDoublesSurface surface)
        {
            Assert.Throws<NotImplementedException>(() => surface.GetZValue(23, 23));
            Assert.NotEmpty(surface.XData);
        }

        public static void AssertSensibleValue(MissingMarketDataSentinel sentinel)
        {
            Assert.Equal(MissingMarketDataSentinel.Instance, sentinel);
            Assert.True(ReferenceEquals(MissingMarketDataSentinel.Instance, sentinel));
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
        public static void AssertSensibleValue(LocalDate date)
        {
            Assert.Equal(date.Date, date.Date.Date);
            AssertSensibleValue(date.Date);
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

        public static void AssertSensibleValue(SABRFittedSurfaces surfaces)
        {
            Assert.NotNull(surfaces);
            AssertSensibleValue(surfaces.AlphaSurface);
            AssertSensibleValue(surfaces.BetaSurface);
            Assert.NotNull(surfaces.Currency);
            AssertSensibleValue(surfaces.DayCountName);
            AssertSensibleValue(surfaces.NuSurface);
            AssertSensibleValue(surfaces.RhoSurface);
        }
        public static void AssertSensibleValue(CurrencyAmount amount)
        {
            Assert.NotNull(amount.Currency);
            AssertSensibleValue(amount.Amount);
        }
        public static void AssertSensibleValue(DoubleMatrix1D matrix)
        {
            AssertSensibleValue(matrix.Data);
        }
        public static void AssertSensibleValue(PresentValueSensitivity sensitivity)
        {
            AssertSensibleValue(sensitivity.Sensitivities);
        }
        public static void AssertSensibleValue(InterpolatedYieldCurveSpecificationWithSecurities spec)
        {
            AssertSensibleValue(spec.Name);
            AssertSensibleValue(spec.Currency);
            AssertSensibleValue(spec.CurveDate);
            AssertSensibleValue(spec.Strips);
        }
        public static void AssertSensibleValue(FixedIncomeStripWithSecurity strip)
        {
            AssertSensibleValue(strip.SecurityIdentifier);
            //These securities are weird AssertSensibleValue(strip.Security);
            Assert.NotNull(strip.Security);
            AssertSensibleValue(strip.OriginalStrip.CurveNodePointTime);
        }

        public static void AssertSensibleValue(FuturePriceCurveData data)
        {
            Assert.NotNull(data.DefinitionName);
            Assert.NotNull(data.SpecificationName);
        }
    }
}
