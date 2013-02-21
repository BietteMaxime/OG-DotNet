// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SnapshotExtensionMethods.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using OpenGamma.Financial.Analytics.Volatility.Cube;
using OpenGamma.Util;

namespace OpenGamma.MarketDataSnapshot.Impl
{
    public static class SnapshotExtensionMethods
    {
        /// <summary>
        /// return a - b
        /// </summary>
        /// <param name="a">The template</param>
        /// <param name="b">The base to subtract</param>
        /// <returns></returns>
        public static ManageableMarketDataSnapshot Substract(this ManageableMarketDataSnapshot a, ManageableMarketDataSnapshot b)
        {
            var globalValues = a.GlobalValues.Subtract(b.GlobalValues);
            var curveSnapshots = Subtract(a.YieldCurves, b.YieldCurves);
            var cubeSnapshots = Subtract(a.VolatilityCubes, b.VolatilityCubes);
            var surfaceSnapshots = Subtract(a.VolatilitySurfaces, b.VolatilitySurfaces);

            return new ManageableMarketDataSnapshot(a.BasisViewName, globalValues, curveSnapshots, cubeSnapshots, surfaceSnapshots);
        }

        private static Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> Subtract(IDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> a, IDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> b)
        {
            return SubtractF(a, b, Subtract);
        }

        private static ManageableVolatilitySurfaceSnapshot Subtract(ManageableVolatilitySurfaceSnapshot a, ManageableVolatilitySurfaceSnapshot b)
        {
            return new ManageableVolatilitySurfaceSnapshot(Subtract(a.Values, b.Values));
        }

        private static Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> Subtract(IDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> a, IDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> b)
        {
            return SubtractF(a, b, Subtract);
        }

        private static ManageableVolatilityCubeSnapshot Subtract(ManageableVolatilityCubeSnapshot a, ManageableVolatilityCubeSnapshot b)
        {
            return new ManageableVolatilityCubeSnapshot(
                Subtract(a.OtherValues, b.OtherValues), 
                Subtract(a.Values, b.Values), 
                Subtract(a.Strikes, b.Strikes));
        }

        private static Dictionary<TKey, ValueSnapshot> Subtract<TKey>(IDictionary<TKey, ValueSnapshot> a, IDictionary<TKey, ValueSnapshot> b)
        {
            return SubtractF(a, b, Subtract);
        }

        private static IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> Subtract(IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> a, IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> b)
        {
            return SubtractF(a, b, Subtract);
        }

        private static ManageableYieldCurveSnapshot Subtract(ManageableYieldCurveSnapshot a, ManageableYieldCurveSnapshot b)
        {
            return new ManageableYieldCurveSnapshot(a.Values.Subtract(b.Values), a.ValuationTime);
        }

        public static ManageableUnstructuredMarketDataSnapshot Subtract(this ManageableUnstructuredMarketDataSnapshot a, ManageableUnstructuredMarketDataSnapshot b)
        {
            IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> aVals = a.Values;
            var bVals = b.Values;

            var subtractF = SubtractF(aVals, bVals, Subtract);
            return new ManageableUnstructuredMarketDataSnapshot(subtractF);
        }

        private static ValueSnapshot Subtract(ValueSnapshot a, ValueSnapshot b)
        {
            ValueSnapshot ret;
            if (a.MarketValue.HasValue && b.MarketValue.HasValue)
            {
                ret = new ValueSnapshot(Subtract(a.MarketValue, b.MarketValue));
            }
            else
            {
                ret = new ValueSnapshot(null);
            }

            if (a.OverrideValue.HasValue)
            {
                if (b.OverrideValue.HasValue || b.MarketValue.HasValue)
                {
                    ret.OverrideValue = Subtract(a.OverrideValue, GetValue(b));
                }
                else
                {
                    ret.OverrideValue = null;
                }
            }

            return ret;
        }

        private static double Subtract(double? x, double? y)
        {
            return Subtract(x.Value, y.Value);
        }

        private static double Subtract(double x, double y)
        {
            // This goes some way to stopping getting stupid recurring decimals
            return (double)(((decimal)x) - ((decimal)y));
        }

        private static Dictionary<TKey, TValue> SubtractF<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b, Func<TValue, TValue, TValue> subtract)
        {
            return a.ProjectStructure(b, 
                (k, x, y) => Tuple.Create<TKey, TValue>(k, subtract(x, y)), 
                (_, __) => null, 
                (_, __) => null)
                .Where(t => t != null).ToDictionary(t => t.Item1, t => t.Item2);
        }

        private static double? GetValue(ValueSnapshot a)
        {
            return a.OverrideValue ?? a.MarketValue;
        }
    }
}
