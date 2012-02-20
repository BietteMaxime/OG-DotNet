//-----------------------------------------------------------------------
// <copyright file="MasterViewTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Model.View;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MasterViewTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanAddUpdateRemove()
        {
            //TODO: make this test resilient to other changes

            using (var remoteClient = Context.CreateFinancialClient())
            {
                var snapshotMaster = remoteClient.MarketDataSnapshotMaster;
                using (var view = MasterView.Create(snapshotMaster))
                {
                    //TODO check events
                    var name = TestUtils.GetUniqueName();

                    AssertValidView(view.Documents);
                    int count = view.Documents.Count;
                    
                    var marketDataSnapshotDocument = snapshotMaster.Add(RemoteMarketDataSnapshotMasterTests.GetDocument(name));
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    AssertValidView(view.Documents);
                    int count2 = view.Documents.Count;
                    Assert.Equal(count + 1, count2);
                    int index = IndexOf(view.Documents, marketDataSnapshotDocument.UniqueId.ObjectID);
                    Assert.InRange(index, 0, view.Documents.Count);

                    marketDataSnapshotDocument.Snapshot.Name = marketDataSnapshotDocument.Snapshot.Name + " Updated";
                    snapshotMaster.Update(marketDataSnapshotDocument);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    AssertValidView(view.Documents);
                    int count4 = view.Documents.Count;
                    Assert.Equal(count2, count4);
                    int updatedIndex = IndexOf(view.Documents, marketDataSnapshotDocument.UniqueId.ObjectID);
                    Assert.Equal(index, updatedIndex);

                    snapshotMaster.Remove(marketDataSnapshotDocument.UniqueId);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    AssertValidView(view.Documents);
                    int count3 = view.Documents.Count;
                    Assert.Equal(count, count3);
                }
            }
        }

        private static int IndexOf(ObservableCollection<MarketDataSnapshotDocument> documents, ObjectId objectID)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i].UniqueId.ObjectID.Equals(objectID))
                {
                    return i;
                }
            }
            return -1;
        }

        private static void AssertValidView(ObservableCollection<MarketDataSnapshotDocument> documents)
        {
            Assert.True(documents.ToLookup(d => d.UniqueId.ObjectID).All(g => g.Count() == 1));
            //TODO sorting
        }
    }
}