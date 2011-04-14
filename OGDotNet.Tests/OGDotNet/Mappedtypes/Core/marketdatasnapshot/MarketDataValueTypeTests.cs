//-----------------------------------------------------------------------
// <copyright file="MarketDataValueTypeTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class MarketDataValueTypeTests
    {
        [Theory]
        [EnumValuesData]
        public void CanRoundTrip(MarketDataValueType type)
        {
            var computationTargetType = EnumUtils<MarketDataValueType, ComputationTargetType>.ConvertTo(type);
            var roundTripped = EnumUtils<ComputationTargetType, MarketDataValueType>.ConvertTo(computationTargetType);
            Assert.Equal(type, roundTripped);
        }
    }
}
