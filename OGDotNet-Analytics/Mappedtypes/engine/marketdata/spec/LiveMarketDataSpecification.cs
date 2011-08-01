//-----------------------------------------------------------------------
// <copyright file="LiveMarketDataSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Engine.marketdata.spec
{
    [FudgeSurrogate(typeof(LiveMarketDataSpecificationBuilder))]
    public class LiveMarketDataSpecification : MarketDataSpecification
    {
        private readonly string _dataSource;

        public LiveMarketDataSpecification() : this(string.Empty)
        {
        }

        public LiveMarketDataSpecification(string dataSource)
        {
            ArgumentChecker.NotNull(dataSource, "dataSource");
            _dataSource = dataSource;
        }

        public string DataSource
        {
            get { return _dataSource; }
        }
    }
}
