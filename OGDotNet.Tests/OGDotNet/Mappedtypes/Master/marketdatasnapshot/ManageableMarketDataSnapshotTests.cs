//-----------------------------------------------------------------------
// <copyright file="ManageableMarketDataSnapshotTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Cube;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Tuple;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    public class ManageableMarketDataSnapshotTests
    {
        [Fact]
        public void CanRemoveAllOverrides()
        {
            var manageableUnstructuredMarketDataSnapshot = new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            var manageableVolatilitySurfaceSnapshots = GetManageableVolatilitySurfaceSnapshots();
            var manageableYieldCurveSnapshots = GetManageableYieldCurveSnapshots();
            var manageableVolatilityCubeSnapshots = GetManageableVolatilityCubeSnapshots();
            var manageableMarketDataSnapshot = new ManageableMarketDataSnapshot("SomeView", manageableUnstructuredMarketDataSnapshot, manageableYieldCurveSnapshots, manageableVolatilityCubeSnapshots, manageableVolatilitySurfaceSnapshots);

            var valueSpec = new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("ID", "1"));
            const string valueName = "Value";
            manageableMarketDataSnapshot.Values.Add(valueSpec, new Dictionary<string, ValueSnapshot> {{valueName, new ValueSnapshot(12){OverrideValue = 13}}});
            Assert.True(manageableMarketDataSnapshot.HaveOverrides());
            Assert.True(manageableMarketDataSnapshot.YieldCurves.Single().Value.HaveOverrides());
            Assert.True(manageableMarketDataSnapshot.VolatilityCubes.Single().Value.HaveOverrides());
            Assert.True(manageableMarketDataSnapshot.VolatilitySurfaces.Single().Value.HaveOverrides());

            manageableMarketDataSnapshot.RemoveAllOverrides();
            var valueSnapshot = manageableMarketDataSnapshot.Values[valueSpec][valueName];
            CheckOverrideCleared(valueSnapshot);
            CheckOverrideCleared(manageableMarketDataSnapshot.YieldCurves.Values.Single().Values.Values.Single().Value.Single().Value);
            CheckOverrideCleared(manageableMarketDataSnapshot.VolatilityCubes.Values.Single().OtherValues.Values.Single().Value.Values.Single());
            CheckOverrideCleared(manageableMarketDataSnapshot.VolatilitySurfaces.Values.Single().Values.Values.Single());
            Assert.Equal(1, manageableMarketDataSnapshot.Values.Count);

            Assert.False(manageableMarketDataSnapshot.HaveOverrides());
            Assert.False(manageableMarketDataSnapshot.YieldCurves.Single().Value.HaveOverrides());
            Assert.False(manageableMarketDataSnapshot.VolatilityCubes.Single().Value.HaveOverrides());
            Assert.False(manageableMarketDataSnapshot.VolatilitySurfaces.Single().Value.HaveOverrides());
        }

        private static void CheckOverrideCleared(ValueSnapshot valueSnapshot)
        {
            Assert.Null(valueSnapshot.OverrideValue);
            Assert.Equal(12, valueSnapshot.MarketValue);
        }

        private static Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> GetManageableVolatilityCubeSnapshots()
        {
            return new Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>
                       {
                           {new VolatilityCubeKey(Currency.USD, "N"), new ManageableVolatilityCubeSnapshot(new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> {{new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("S", "V")), new Dictionary<string, ValueSnapshot> {{"K", new ValueSnapshot(12){OverrideValue = 13}}}}}))
                               }
                       };
        }

        private static Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> GetManageableVolatilitySurfaceSnapshots()
        {
            return new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>
                       {
                           {new VolatilitySurfaceKey(UniqueIdentifier.Of("S", "V"), "N", "I"),
                               new ManageableVolatilitySurfaceSnapshot(new Dictionary<Pair<object, object>, ValueSnapshot>
                                                                           {
                                                                               {
                                                                                   new Pair<object, object>(1, 1), new ValueSnapshot(12){OverrideValue = 13}
                                                                                   }
                                                                           })}
                       };
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetManageableYieldCurveSnapshots()
        {
            return new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>
                       {
                           {new YieldCurveKey(Currency.USD, "N"), new ManageableYieldCurveSnapshot(new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>
                                                                                                                                                    {
                                                                                                                                                                                                                                      {
                                                                                                                                                                                                                                          new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("S", "V")), new Dictionary<string, ValueSnapshot> {{"K", new ValueSnapshot(12){OverrideValue = 13}}}
                                                                                                                                                                                                                                          }}), DateTimeOffset.Now)}
                       };
        }
    }
}
