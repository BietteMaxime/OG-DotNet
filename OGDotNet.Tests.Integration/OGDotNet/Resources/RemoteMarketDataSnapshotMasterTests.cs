//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Db;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteMarketDataSnapshotMasterTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanSearch()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;
            var searchResult = snapshotMaster.Search(TestUtils.GetUniqueName(), new PagingRequest(1, 10));
            Assert.NotNull(searchResult.Documents);
            Assert.Empty(searchResult.Documents);
        }

        [Xunit.Extensions.Fact]
        public void SearchMatchesMetadata()
        {
            var pagingRequest = new PagingRequest(1, 2);

            var snapshotMaster = Context.MarketDataSnapshotMaster;
            var searchResult = snapshotMaster.Search("*", pagingRequest);
            Assert.NotNull(searchResult.Documents);
            Assert.NotEmpty(searchResult.Documents);

            var metaSearchResult = snapshotMaster.SearchMetadata("*", pagingRequest);

            Assert.Equal(metaSearchResult.Paging.TotalItems, searchResult.Paging.TotalItems);

            foreach (var t in metaSearchResult.Documents.Zip(searchResult.Documents, Tuple.Create))
            {
                Assert.Equal(t.Item1.CorrectionFromInstant, t.Item2.CorrectionFromInstant);
                Assert.Equal(t.Item1.CorrectionToInstant, t.Item2.CorrectionToInstant);
                Assert.Equal(t.Item1.UniqueId, t.Item2.UniqueId);
                Assert.Equal(t.Item1.VersionFromInstant, t.Item2.VersionFromInstant);
                Assert.Equal(t.Item1.VersionToInstant, t.Item2.VersionToInstant);
                Assert.Equal(t.Item1.Name, t.Item2.Snapshot.Name);
            }
        }

        [Xunit.Extensions.Fact]
        public void MetadataSearchIsFaster()
        {
            var pagingRequest = PagingRequest.All;

            var snapshotMaster = Context.MarketDataSnapshotMaster;


            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var metaSearchResult = snapshotMaster.SearchMetadata("*", pagingRequest);
            var metadataTime = stopwatch.Elapsed;

            stopwatch.Restart();
            var searchResult = snapshotMaster.Search("*", pagingRequest);
            var fullTime = stopwatch.Elapsed;

            Assert.InRange(metadataTime, TimeSpan.Zero, TimeSpan.FromTicks(fullTime.Ticks / 3));
        }

        [Xunit.Extensions.Fact]
        public void CanCreate()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;
            Assert.NotNull(snapshotMaster);
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndRemove()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));

            snapshotMaster.Remove(marketDataSnapshotDocument.UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndSearch()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));

            var searchResult = snapshotMaster.Search(name, new PagingRequest(1, 10));
            Assert.NotEmpty(searchResult.Documents);
            Assert.Equal(1, searchResult.Documents.Count);
            var retDoc = searchResult.Documents[0];

            Assert.NotNull(retDoc.VersionFromInstant);
            Assert.NotNull(retDoc.CorrectionFromInstant);
            Assert.Equal(DateTimeOffset.MinValue, retDoc.VersionToInstant);
            Assert.Equal(DateTimeOffset.MinValue, retDoc.CorrectionToInstant);

            AssertEqual(retDoc, marketDataSnapshotDocument);

            snapshotMaster.Remove(marketDataSnapshotDocument.UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndGetGlobal()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            CanAddAndGet(snapshotMaster);
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndGetUser()
        {
            using (var remoteClient = Context.CreateUserClient())
            {
                var snapshotMaster = remoteClient.MarketDataSnapshotMaster;
                CanAddAndGet(snapshotMaster);
            }
        }

        private static void CanAddAndGet(RemoteMarketDataSnapshotMaster snapshotMaster)
        {
            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));

            var retDoc = snapshotMaster.Get(marketDataSnapshotDocument.UniqueId);
            Assert.NotNull(retDoc);
            Assert.Equal(DateTimeOffset.MinValue, retDoc.VersionFromInstant);
            Assert.Equal(DateTimeOffset.MinValue, retDoc.CorrectionFromInstant);

            AssertEqual(retDoc, marketDataSnapshotDocument);

            snapshotMaster.Remove(marketDataSnapshotDocument.UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanUpdate()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));
            UniqueIdentifier init = marketDataSnapshotDocument.UniqueId;
            Assert.True(init.IsVersioned);
            Assert.Equal("0", init.Version);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, marketDataSnapshotDocument.Snapshot.UniqueId);

            snapshotMaster.Update(marketDataSnapshotDocument);

            UniqueIdentifier afterUpdate = marketDataSnapshotDocument.UniqueId;
            Assert.True(afterUpdate.IsVersioned);
            Assert.Equal("1", afterUpdate.Version);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, marketDataSnapshotDocument.Snapshot.UniqueId);

            snapshotMaster.Remove(afterUpdate);
        }

        [Xunit.Extensions.Fact(Skip = "This isn't a test, but it's quite useful")]
        public void CanRemoveAllMine()
        {
            string searchString = string.Format("*{0}*", GetType().Name);

            RemoveAll(searchString);
        }

        [Xunit.Extensions.Fact(Skip = "This isn't a test, but it's quite useful")]
        public void CanRemoveAll()
        {
            const string searchString = "*";

            RemoveAll(searchString);
        }

        private static void RemoveAll(string searchString)
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var searchResult = snapshotMaster.Search(searchString, PagingRequest.All);
            foreach (var dataSnapshotDocument in searchResult.Documents)
            {
                snapshotMaster.Remove(dataSnapshotDocument.UniqueId);
            }
        }

        private static MarketDataSnapshotDocument GetDocument(string name)
        {
            var manageableUnstructuredMarketDataSnapshot = new ManageableUnstructuredMarketDataSnapshot(
                new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>
                    {
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("AA", "XX")),
                            new Dictionary<string, ValueSnapshot>
                                {
                                    {"SomeValue", new ValueSnapshot(12)}
                                }
                            },
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Of("AA", "YY")),
                            new Dictionary<string, ValueSnapshot>
                                {
                                    {"SomeOtherValue", new ValueSnapshot(12){OverrideValue = 13}}
                                }
                            }
                    }
                );
            return new MarketDataSnapshotDocument(null,
                new ManageableMarketDataSnapshot("SomeView",
                    manageableUnstructuredMarketDataSnapshot,
                    new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>
                        {
                            {new YieldCurveKey(Currency.USD, "Default"), new ManageableYieldCurveSnapshot(manageableUnstructuredMarketDataSnapshot, DateTimeOffset.Now)}
                        }
                    ) { Name = name });
        }

        private static void AssertEqual(MarketDataSnapshotDocument retDoc, MarketDataSnapshotDocument marketDataSnapshotDocument)
        {
            Assert.NotNull(retDoc.UniqueId);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, retDoc.UniqueId);

            AssertEqual(retDoc.Snapshot, marketDataSnapshotDocument.Snapshot);
        }

        private static void AssertEqual(ManageableMarketDataSnapshot retSnapshot, ManageableMarketDataSnapshot mSnapshot)
        {
            Assert.NotNull(retSnapshot);
            Assert.Equal(mSnapshot.Name, retSnapshot.Name);
            Assert.NotNull(retSnapshot.Values);
            Assert.Equal(mSnapshot.BasisViewName, retSnapshot.BasisViewName);

            AssertEqual(mSnapshot.GlobalValues, retSnapshot.GlobalValues);

            AssertEqual(mSnapshot.YieldCurves, retSnapshot.YieldCurves, AssertEqual);

            // TODO volatility surfaces etc.
        }

        private static void AssertEqual(ManageableYieldCurveSnapshot a, ManageableYieldCurveSnapshot b)
        {
            AssertEqual(a.Values, b.Values);
            var timeSpan = a.ValuationTime.LocalDateTime - b.ValuationTime.LocalDateTime;
            Assert.InRange(timeSpan.TotalMilliseconds, 0, 1000); //Only second accuracy
        }

        private static void AssertEqual(ManageableUnstructuredMarketDataSnapshot a, ManageableUnstructuredMarketDataSnapshot b)
        {
            AssertEqual(a.Values, b.Values, AssertEqual);
        }

        private static void AssertEqual(IDictionary<string, ValueSnapshot> a, IDictionary<string, ValueSnapshot> b)
        {
            AssertEqual(a, b, AssertEqual);
        }

        private static void AssertEqual(ValueSnapshot a, ValueSnapshot b)
        {
            Assert.Equal(a.MarketValue, b.MarketValue);
            Assert.Equal(a.OverrideValue, b.OverrideValue);
        }

        private static void AssertEqual<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b, Action<TValue, TValue> assert)
        {
            Assert.Equal(a.Keys.OrderBy(x => x.GetHashCode()), b.Keys.OrderBy(x => x.GetHashCode()));

            foreach (var t in a.Join(b, aP => aP.Key, bP => bP.Key, Tuple.Create))
            {
                assert(t.Item1.Value, t.Item2.Value);
            }
        }
    }
}
