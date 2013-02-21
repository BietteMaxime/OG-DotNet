// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using OpenGamma.Core.Change;
using OpenGamma.Financial.Analytics.Volatility.Cube;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot;
using OpenGamma.MarketDataSnapshot.Impl;
using OpenGamma.Master.MarketDataSnapshot;
using OpenGamma.Util;
using OpenGamma.Util.Money;
using OpenGamma.Util.Time;
using OpenGamma.Util.Tuple;
using OpenGamma.Xunit.Extensions;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class RemoteMarketDataSnapshotMasterTests : RemoteEngineContextTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanSearch()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;
            var searchRequest = new MarketDataSnapshotSearchRequest(TestUtils.GetUniqueName(), PagingRequest.First(10));
            var searchResult = snapshotMaster.Search(searchRequest);
            Assert.NotNull(searchResult.Documents);
            Assert.Empty(searchResult.Documents);
        }

        [Xunit.Extensions.Fact]
        public void SearchMatchesNotInclude()
        {
            var pagingRequest = PagingRequest.First(2);
            var snapshotMaster = Context.MarketDataSnapshotMaster;
            var searchRequest = new MarketDataSnapshotSearchRequest("*", pagingRequest);
            var searchResult = snapshotMaster.Search(searchRequest);
            Assert.NotNull(searchResult.Documents);
            Assert.NotEmpty(searchResult.Documents);

            var metaSearchResult = snapshotMaster.Search(new MarketDataSnapshotSearchRequest("*", pagingRequest, false));

            Assert.Equal(metaSearchResult.Paging.TotalItems, searchResult.Paging.TotalItems);

            foreach (var t in metaSearchResult.Documents.Zip(searchResult.Documents, Tuple.Create))
            {
                Assert.Equal(t.Item1.CorrectionFromInstant, t.Item2.CorrectionFromInstant);
                Assert.Equal(t.Item1.CorrectionToInstant, t.Item2.CorrectionToInstant);
                Assert.Equal(t.Item1.UniqueId, t.Item2.UniqueId);
                Assert.Equal(t.Item1.VersionFromInstant, t.Item2.VersionFromInstant);
                Assert.Equal(t.Item1.VersionToInstant, t.Item2.VersionToInstant);
                Assert.Equal(t.Item1.Snapshot.Name, t.Item2.Snapshot.Name);
                Assert.Empty(t.Item1.Snapshot.YieldCurves);
                Assert.Null(t.Item1.Snapshot.GlobalValues);
            }
        }

        [Xunit.Extensions.Fact(Skip = "Inconsistent behaviour")]
        public void NotIncludeSearchIsFaster()
        {
            var pagingRequest = PagingRequest.All;

            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var metaSearchResult = snapshotMaster.Search(new MarketDataSnapshotSearchRequest("*", pagingRequest, false));
            Assert.NotNull(metaSearchResult);
            var metadataTime = stopwatch.Elapsed;

            if (metaSearchResult.Documents.Count == 0)
            {
                // Test is pointless
                return;
            }

            stopwatch.Restart();
            var searchRequest = new MarketDataSnapshotSearchRequest("*", pagingRequest);
            var searchResult = snapshotMaster.Search(searchRequest);
            Assert.NotNull(searchResult);
            var fullTime = stopwatch.Elapsed;

            Assert.True(metadataTime < fullTime);
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

            var searchRequest = new MarketDataSnapshotSearchRequest(name, PagingRequest.First(10));
            var searchResult = snapshotMaster.Search(searchRequest);
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
            using (var remoteClient = Context.CreateFinancialClient())
            {
                var snapshotMaster = remoteClient.MarketDataSnapshotMaster;
                CanAddAndGet(snapshotMaster);
            }
        }

        private static void CanAddAndGet(RemoteMarketDataSnapshotMaster snapshotMaster)
        {
            /*
            var changeManager = snapshotMaster.ChangeManager;
            var aggregatingChangeListener = new AggregatingChangeListener();
            changeManager.AddChangeListener(aggregatingChangeListener);
            try
            {
             */
                var name = TestUtils.GetUniqueName();

                var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));

                Thread.Sleep(TimeSpan.FromSeconds(1));
                //List<ChangeEvent> events = aggregatingChangeListener.GetAndClearEvents();
                //var addEvent = events.Single(e => e.Type == ChangeType.Added && e.AfterId.Equals(marketDataSnapshotDocument.UniqueId));

                var retDoc = snapshotMaster.Get(marketDataSnapshotDocument.UniqueId);
                Assert.NotNull(retDoc);
                Assert.NotEqual(DateTimeOffset.MinValue, retDoc.VersionFromInstant);
                Assert.NotEqual(DateTimeOffset.MinValue, retDoc.CorrectionFromInstant);

                //Assert.Equal(retDoc.VersionFromInstant, addEvent.VersionInstant.ToDateTimeOffset());

                AssertEqual(retDoc, marketDataSnapshotDocument);

                snapshotMaster.Remove(marketDataSnapshotDocument.UniqueId);

                Thread.Sleep(TimeSpan.FromSeconds(1));
                //events = aggregatingChangeListener.GetAndClearEvents();
                //var removeEvent = events.Single(e => e.Type == ChangeType.Removed && e.BeforeId.Equals(marketDataSnapshotDocument.UniqueId));

                //var deleted = snapshotMaster.Get(marketDataSnapshotDocument.UniqueId);
                //Assert.Equal(deleted.VersionToInstant, removeEvent.VersionInstant.ToDateTimeOffset());
            /*
            }
            finally
            {
                changeManager.RemoveChangeListener(aggregatingChangeListener);
            }
             */
        }

        [Xunit.Extensions.Fact]
        public void CanGetHistory()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));
            UniqueId init = marketDataSnapshotDocument.UniqueId;
            Assert.True(init.IsVersioned);
            Assert.Equal("0", init.Version);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, marketDataSnapshotDocument.Snapshot.UniqueId);

            snapshotMaster.Update(marketDataSnapshotDocument);

            UniqueId afterUpdate = marketDataSnapshotDocument.UniqueId;
            Assert.True(afterUpdate.IsVersioned);
            Assert.Equal("1", afterUpdate.Version);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, marketDataSnapshotDocument.Snapshot.UniqueId);

            snapshotMaster.Remove(afterUpdate);

            var marketDataSnapshotHistoryResult = snapshotMaster.History(new MarketDataSnapshotHistoryRequest(init.ObjectId, false));
            Assert.Equal(2, marketDataSnapshotHistoryResult.Documents.Count);
            Assert.Equal(init, marketDataSnapshotHistoryResult.Documents.Last().UniqueId);
            Assert.Equal(afterUpdate, marketDataSnapshotHistoryResult.Documents.First().UniqueId);
        }

        [Xunit.Extensions.Fact]
        public void CanUpdate()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

            var name = TestUtils.GetUniqueName();

            var marketDataSnapshotDocument = snapshotMaster.Add(GetDocument(name));
            UniqueId init = marketDataSnapshotDocument.UniqueId;
            Assert.True(init.IsVersioned);
            Assert.Equal("0", init.Version);
            Assert.Equal(marketDataSnapshotDocument.UniqueId, marketDataSnapshotDocument.Snapshot.UniqueId);

            snapshotMaster.Update(marketDataSnapshotDocument);

            UniqueId afterUpdate = marketDataSnapshotDocument.UniqueId;
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

            var searchRequest = new MarketDataSnapshotSearchRequest(searchString, PagingRequest.All);
            var searchResult = snapshotMaster.Search(searchRequest);
            foreach (var dataSnapshotDocument in searchResult.Documents)
            {
                snapshotMaster.Remove(dataSnapshotDocument.UniqueId);
            }
        }

        public static MarketDataSnapshotDocument GetDocument(string name)
        {
            return new MarketDataSnapshotDocument(null, 
                new ManageableMarketDataSnapshot("SomeView", 
                    GetGlobal(), 
                    GetYieldCurves(GetGlobal()), 
                    GetCubes(), 
                        GetSurfaces()
                    ) {
                         Name = name 
                      });
        }

        private static ManageableUnstructuredMarketDataSnapshot GetGlobal()
        {
            return new ManageableUnstructuredMarketDataSnapshot(
                new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>
                    {
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Create("AA", "XX")), 
                            new Dictionary<string, ValueSnapshot>
                                {
                                    {"SomeValue", new ValueSnapshot(12)}
                                }
                            }, 
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueId.Create("AA", "YY")), 
                            new Dictionary<string, ValueSnapshot>
                                {
                                    {"SomeOtherValue", new ValueSnapshot(12){OverrideValue = 13}}
                                }
                            }
                    }
                );
        }

        private static Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot> GetYieldCurves(ManageableUnstructuredMarketDataSnapshot manageableUnstructuredMarketDataSnapshot)
        {
            return new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>
                       {
                           {new YieldCurveKey(Currency.USD, "Default"), new ManageableYieldCurveSnapshot(manageableUnstructuredMarketDataSnapshot, DateTimeOffset.Now)}
                       };
        }

        private static Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> GetCubes()
        {
            var manageableVolCubeSnapshot = new ManageableVolatilityCubeSnapshot(new ManageableUnstructuredMarketDataSnapshot(new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>()));
            manageableVolCubeSnapshot.SetPoint(new VolatilityPoint(Tenor.Day, Tenor.Day, -100), new ValueSnapshot(2));
            manageableVolCubeSnapshot.SetPoint(new VolatilityPoint(Tenor.Day, Tenor.Day, 100), null);
            manageableVolCubeSnapshot.SetStrike(new Pair<Tenor, Tenor>(Tenor.Day, Tenor.Day), new ValueSnapshot(2));
            manageableVolCubeSnapshot.SetStrike(new Pair<Tenor, Tenor>(Tenor.Day, Tenor.TwoYears), null);
            return new Dictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>
                       {
                           {new VolatilityCubeKey(Currency.USD, "Test"), manageableVolCubeSnapshot}
                       };
        }

        private static Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> GetSurfaces()
        {
            return new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>
                       {
                           {
                               new VolatilitySurfaceKey(Currency.GBP.UniqueId, "TEST", "SOME INSTRUMENT TYPE", 
                                                        "QUOTE_TYPE", "QUOTE_UNITS"), 
                               new ManageableVolatilitySurfaceSnapshot(new Dictionary<Pair<object, object>, ValueSnapshot>
                                                                           {
                                                                               {
                                                                                   new Pair<object, object>("A", "B"), 
                                                                                   new ValueSnapshot(12)
                                                                                   }, 
                                                                               {
                                                                                   new Pair<object, object>("A", "C"), 
                                                                                   new ValueSnapshot(12)
                                                                                   }
                                                                           })
                               }
                       };
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

            AssertEqual(mSnapshot.VolatilityCubes, retSnapshot.VolatilityCubes, AssertEqual);

            // TODO volatility surfaces etc.
        }

        private static void AssertEqual(ManageableVolatilityCubeSnapshot a, ManageableVolatilityCubeSnapshot b)
        {
            Assert.NotNull(a.OtherValues);
            Assert.NotNull(b.OtherValues);
            AssertEqual(a.Values, b.Values, AssertEqual);
            AssertEqual(a.OtherValues, b.OtherValues);
            AssertEqual(a.Strikes, b.Strikes, AssertEqual);
        }

        private static void AssertEqual(ManageableYieldCurveSnapshot a, ManageableYieldCurveSnapshot b)
        {
            AssertEqual(a.Values, b.Values);
            var timeSpan = a.ValuationTime.LocalDateTime - b.ValuationTime.LocalDateTime;
            Assert.InRange(timeSpan.TotalMilliseconds, 0, 1000); // Only second accuracy
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
            if (a == null || b == null)
            {
                Assert.Null(a);
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.MarketValue, b.MarketValue);
                Assert.Equal(a.OverrideValue, b.OverrideValue);
            }
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
