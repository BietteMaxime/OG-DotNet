//-----------------------------------------------------------------------
// <copyright file="LiveMarketDataSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Engine.MarketData.Spec
{
    [FudgeSurrogate(typeof(LiveMarketDataSpecificationBuilder))]
    public class LiveMarketDataSpecification : MarketDataSpecification
    {
        private readonly string _dataSource;

        public LiveMarketDataSpecification() : this(null)
        {
        }

        public LiveMarketDataSpecification(string dataSource)
        {
            _dataSource = dataSource;
        }

        public string DataSource
        {
            get { return _dataSource; }
        }
    }
}
