//-----------------------------------------------------------------------
// <copyright file="CombinedMarketDataSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Engine.MarketData.Spec
{
    [FudgeSurrogate(typeof(CombinedMarketDataSpecificationBuilder))]
    public class CombinedMarketDataSpecification : MarketDataSpecification
    {
        private readonly MarketDataSpecification _preferredSpecification;
        private readonly MarketDataSpecification _fallbackSpecification;

        public CombinedMarketDataSpecification(MarketDataSpecification preferredSpecification, MarketDataSpecification fallbackSpecification)
        {
            _preferredSpecification = preferredSpecification;
            _fallbackSpecification = fallbackSpecification;
        }

        public MarketDataSpecification PreferredSpecification
        {
            get { return _preferredSpecification; }
        }

        public MarketDataSpecification FallbackSpecification
        {
            get { return _fallbackSpecification; }
        }
    }
}
