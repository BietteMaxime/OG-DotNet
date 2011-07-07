//-----------------------------------------------------------------------
// <copyright file="BloombergFXOptionVolatilitySurfaceInstrumentProvider.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface
{
    public static class BloombergFXOptionVolatilitySurfaceInstrumentProvider
    {
        [FudgeSurrogate(typeof(EnumBuilder<FXVolQuoteType>))]
        public enum FXVolQuoteType
        {
            ATM,
            RiskReversal,
            Butterfly
        }
    }
}
