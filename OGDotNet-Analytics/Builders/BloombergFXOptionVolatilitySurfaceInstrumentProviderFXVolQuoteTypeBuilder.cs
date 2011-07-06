//-----------------------------------------------------------------------
// <copyright file="BloombergFXOptionVolatilitySurfaceInstrumentProviderFXVolQuoteTypeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;

namespace OGDotNet.Builders
{
    class BloombergFXOptionVolatilitySurfaceInstrumentProviderFXVolQuoteTypeBuilder : BuilderBase<BloombergFXOptionVolatilitySurfaceInstrumentProvider.FXVolQuoteType>
    {
        public BloombergFXOptionVolatilitySurfaceInstrumentProviderFXVolQuoteTypeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override BloombergFXOptionVolatilitySurfaceInstrumentProvider.FXVolQuoteType DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return
                new BloombergFXOptionVolatilitySurfaceInstrumentProvider.FXVolQuoteType(msg.GetString(1));
        }
    }
}
