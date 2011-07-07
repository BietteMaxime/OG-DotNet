//-----------------------------------------------------------------------
// <copyright file="BloombergFXOptionVolatilitySurfaceInstrumentProvider.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface
{
    public static class BloombergFXOptionVolatilitySurfaceInstrumentProvider
    {
        [FudgeSurrogate(typeof(BloombergFXOptionVolatilitySurfaceInstrumentProviderFXVolQuoteTypeBuilder))]
        public class FXVolQuoteType : IEquatable<FXVolQuoteType>
        {
            private readonly string _name;

            public FXVolQuoteType(string name)
            {
                ArgumentChecker.NotNull(name, "name");
                _name = name;
            }

            public string Name
            {
                get { return _name; }
            }

            public bool Equals(FXVolQuoteType other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._name, _name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(FXVolQuoteType)) return false;
                return Equals((FXVolQuoteType) obj);
            }

            public override int GetHashCode()
            {
                return _name.GetHashCode();
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
