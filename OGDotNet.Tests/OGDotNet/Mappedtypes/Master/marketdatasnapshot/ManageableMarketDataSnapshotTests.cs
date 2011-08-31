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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Cube;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model.Context.MarketDataSnapshot;
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

            var valueSpec = new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("ID", "1"));
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
                           {new VolatilityCubeKey(Currency.USD, "N"), new ManageableVolatilityCubeSnapshot(new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> {{new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("S", "V")), new Dictionary<string, ValueSnapshot> {{"K", new ValueSnapshot(12){OverrideValue = 13}}}}}))
                               }
                       };
        }

        private static Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> GetManageableVolatilitySurfaceSnapshots()
        {
            return new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>
                       {
                           {new VolatilitySurfaceKey(UniqueId.Of("S", "V"), "N", "I"),
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
                                                                                                                                                                                                                                          new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("S", "V")), new Dictionary<string, ValueSnapshot> {{"K", new ValueSnapshot(12){OverrideValue = 13}}}
                                                                                                                                                                                                                                          }}), DateTimeOffset.Now)}
                       };
        }

        [Fact]
        public void VersionedSecuritiesUpdateCorrectlyBasic() //LAP-30
        {
            var before =
                new ManageableUnstructuredMarketDataSnapshot(
                    new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            var update =
                new ManageableUnstructuredMarketDataSnapshot(
                    new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            before.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("A", "A", "1")), new Dictionary<string, ValueSnapshot> { { "A", new ValueSnapshot(null) } });
            update.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("A", "A", "2")), new Dictionary<string, ValueSnapshot> { { "A", new ValueSnapshot(null) } });

            UpdateAction<ManageableUnstructuredMarketDataSnapshot> prepareUpdateFrom = before.PrepareUpdateFrom(update);
            Assert.Equal(0, prepareUpdateFrom.Warnings.Count());

            prepareUpdateFrom.Execute(before);

            List<UniqueId> updatedIds = before.Values.Keys.Select(m => m.UniqueId).OrderBy(u => u).ToList();
            List<UniqueId> expected = new[]
                                          {
                                              UniqueId.Of("A", "A", "2"),
                                          }.OrderBy(u => u).ToList();

            Assert.Equal(expected.Count, updatedIds.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], updatedIds[i]);
            }
        }

        [Fact]
        public void VersionedSecuritiesUpdateCorrectly() //LAP-30
        {
            var before =
                new ManageableUnstructuredMarketDataSnapshot(
                    new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            var update =
                new ManageableUnstructuredMarketDataSnapshot(
                    new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>());
            before.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("A", "A", "1")), new Dictionary<string, ValueSnapshot>{{"A", new ValueSnapshot(null)}});
            update.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("A", "A", "2")), new Dictionary<string, ValueSnapshot> { { "A", new ValueSnapshot(null) } });
            
            before.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("B", "B", "1")), new Dictionary<string, ValueSnapshot> { { "B", new ValueSnapshot(null) } });
            update.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("B", "B", "1")), new Dictionary<string, ValueSnapshot> { { "B", new ValueSnapshot(null) } });
            before.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("B", "B", "2")), new Dictionary<string, ValueSnapshot> { { "B", new ValueSnapshot(null) } });
            update.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("B", "B", "2")), new Dictionary<string, ValueSnapshot> { { "B", new ValueSnapshot(null) } });

            before.Values.Add(new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Of("C", "C", "1")), new Dictionary<string, ValueSnapshot> { { "C", new ValueSnapshot(null) { OverrideValue = 12 } } });

            UpdateAction<ManageableUnstructuredMarketDataSnapshot> prepareUpdateFrom = before.PrepareUpdateFrom(update);
            Assert.Equal(1, prepareUpdateFrom.Warnings.Count());

            prepareUpdateFrom.Execute(before);

            List<UniqueId> updatedIds = before.Values.Keys.Select(m => m.UniqueId).OrderBy(u => u).ToList();
            List<UniqueId> expected = new[]
                                          {
                                              UniqueId.Of("A", "A", "2"),
                                              UniqueId.Of("B", "B", "1"),
                                              UniqueId.Of("B", "B", "2"),
                                          }.OrderBy(u => u).ToList();

            Assert.Equal(expected.Count, updatedIds.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], updatedIds[i]);
            }
        }
    }
}
