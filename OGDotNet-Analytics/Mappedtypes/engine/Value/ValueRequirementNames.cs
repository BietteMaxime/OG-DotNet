//-----------------------------------------------------------------------
// <copyright file="ValueRequirementNames.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Engine.Value
{
    public static class ValueRequirementNames
    {
        public static readonly string MarketValue = "Market_Value";
        public static readonly string Volume = "Market_Volume";
        public static readonly string ImpliedVolatility = "Market_ImpliedVolatility";

        public static readonly string YieldCurve = "YieldCurve";
        public static readonly string YieldCurveSpec = "YieldCurveSpec";
        public static readonly string YieldCurveInterpolated = "YieldCurveInterpolated";
        public static readonly string YieldCurveMarketData = "YieldCurveMarketData";
        public static readonly string YieldCurveJacobian = "YieldCurveJacobian";

        public static readonly string VolatilitySurfaceData = "VolatilitySurfaceData";

        public static readonly string VolatilityCubeMarketData = "VolatilityCubeMarketData";

        public static readonly string CurveCalculationMethod = "CurveCalculationMethod";
    }
}
