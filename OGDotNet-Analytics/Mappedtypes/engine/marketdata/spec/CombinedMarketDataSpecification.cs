//-----------------------------------------------------------------------
// <copyright file="CombinedMarketDataSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.marketdata.spec
{
    [FudgeSurrogate(typeof(CombinedMarketDataSpecificationBuilder))]
    public class CombinedMarketDataSpecification : MarketDataSpecification
    {
        private readonly MarketDataSpecification _prefferedSpecification;
        private readonly MarketDataSpecification _fallbackSpecification;

        public CombinedMarketDataSpecification(MarketDataSpecification prefferedSpecification, MarketDataSpecification fallbackSpecification)
        {
            _prefferedSpecification = prefferedSpecification;
            _fallbackSpecification = fallbackSpecification;
        }

        public MarketDataSpecification PrefferedSpecification
        {
            get { return _prefferedSpecification; }
        }

        public MarketDataSpecification FallbackSpecification
        {
            get { return _fallbackSpecification; }
        }
    }
}
