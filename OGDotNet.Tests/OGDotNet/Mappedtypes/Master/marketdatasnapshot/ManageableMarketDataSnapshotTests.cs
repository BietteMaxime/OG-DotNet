//-----------------------------------------------------------------------
// <copyright file="ManageableMarketDataSnapshotTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    public class ManageableMarketDataSnapshotTests
    {
        [Fact]
        public void CanRemoveAllOverrides()
        {
            var manageableUnstructuredMarketDataSnapshot = new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            var manageableMarketDataSnapshot = new ManageableMarketDataSnapshot("SomeView", manageableUnstructuredMarketDataSnapshot, new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>(), new Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>(), new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>());

            var valueSpec = new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("ID", "1"));
            const string valueName = "Value";
            manageableMarketDataSnapshot.Values.Add(valueSpec, new Dictionary<string, ValueSnapshot>(){{valueName, new ValueSnapshot(12){OverrideValue = 13}}});
            manageableMarketDataSnapshot.RemoveAllOverrides();
            Assert.Null(manageableMarketDataSnapshot.Values[valueSpec][valueName].OverrideValue);
            Assert.Equal(12, manageableMarketDataSnapshot.Values[valueSpec][valueName].MarketValue);
            Assert.Equal(1, manageableMarketDataSnapshot.Values.Count);
        }
    }
}
