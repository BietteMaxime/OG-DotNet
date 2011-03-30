using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Db;
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
        public void CanAddAndGet()
        {
            var snapshotMaster = Context.MarketDataSnapshotMaster;

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
            string searchString = string.Format("*", GetType().Name);

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
                new Dictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>>()
                    {
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Parse("AA::XX")),
                            new Dictionary<string,ValueSnapshot>()
                                {
                                    {"SomeValue", new ValueSnapshot(12)}
                                }
                            },
                        {
                            new MarketDataValueSpecification(MarketDataValueType.Primitive, UniqueIdentifier.Parse("AA::YY")),
                            new Dictionary<string,ValueSnapshot>()
                                {
                                    {"SomeOtherValue", new ValueSnapshot(12){OverrideValue = 13}}
                                }
                            }
                    }
                );
            return new MarketDataSnapshotDocument(null,
                new ManageableMarketDataSnapshot("SomeView", 
                    manageableUnstructuredMarketDataSnapshot, 
                    new Dictionary<YieldCurveKey, ManageableYieldCurveSnapshot>()
                        {
                            {new YieldCurveKey(Currency.Create("USD"), "Default"), new ManageableYieldCurveSnapshot(manageableUnstructuredMarketDataSnapshot, DateTimeOffset.Now)}
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

            Assert.Equal(mSnapshot.Values.Keys.OrderBy(x => x.GetHashCode()), retSnapshot.Values.Keys.OrderBy(x => x.GetHashCode()));

            //TODO
        }

    }
}
