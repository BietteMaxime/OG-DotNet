//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotManagerTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class MarketDataSnapshotManagerTests : ViewTestsBase
    {
        

        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void CanCreateFromView(RemoteView view)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromView(view))
            {
                var manageableMarketDataSnapshot = proc.Snapshot;
                Assert.Null(manageableMarketDataSnapshot.Name);
                Assert.Null(manageableMarketDataSnapshot.UniqueId);

                Assert.NotEmpty(manageableMarketDataSnapshot.Values);
                foreach (var valueSnapshot in manageableMarketDataSnapshot.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                        Assert.Null(snapshot.Value.OverrideValue);
                    }
                }

                Assert.InRange(manageableMarketDataSnapshot.YieldCurves.Count, ExpectedYieldCurves(view),
                               int.MaxValue);
                if (view.Name == "Equity Option Test View 1")
                {
                    Assert.Equal(1, manageableMarketDataSnapshot.YieldCurves.Count);
                    var yieldCurveSnapshot =
                        manageableMarketDataSnapshot.YieldCurves[new YieldCurveKey(Currency.Create("USD"), "SINGLE")
                            ];
                    Assert.NotNull(yieldCurveSnapshot);
                }
                foreach (var curve in manageableMarketDataSnapshot.YieldCurves.Values)
                {
                    AssertSaneValue(curve);
                }
            }
        }

        private static int ExpectedYieldCurves(RemoteView view)
        {
            return
                view.Definition.CalculationConfigurationsByName.Select(
                    c => c.Value.SpecificRequirements.Where(r => r.ValueName.Contains("YieldCurve")).Count()).Distinct().Single();
        }

        [Theory]
        [TypedPropertyData("FastTickingViews")]
        public void CanUpdateFromView(RemoteView view)
        {
            var snapshotManager = Context.MarketDataSnapshotManager;
            using (var proc = snapshotManager.CreateFromView(view))
            {
                var updated = proc.Snapshot;

                var targetChanged = updated.Values.Keys.First();
                var valueChanged = updated.Values[targetChanged].Keys.First();

                updated.Values[targetChanged][valueChanged].OverrideValue = 12;

                bool seenUpdate = false;
                foreach (var value in updated.GlobalValues.Values.SelectMany(v => v.Value.Values))
                {
                    value.PropertyChanged += delegate { seenUpdate = true; };
                }

                var action = proc.PrepareUpdate();
                Assert.Empty(action.Warnings);
                Assert.False(seenUpdate);
                action.Execute();
                Assert.True(seenUpdate);

                Assert.Null(updated.Name);
                Assert.Null(updated.UniqueId);

                Assert.NotEmpty(updated.Values);

                foreach (var valueSnapshot in updated.Values)
                {
                    foreach (var snapshot in valueSnapshot.Value)
                    {
                        ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                        if (targetChanged == valueSnapshot.Key && valueChanged == snapshot.Key)
                        {
                            Assert.Equal(12, snapshot.Value.OverrideValue);
                        }
                        else
                        {
                            Assert.Null(snapshot.Value.OverrideValue);
                        }
                    }
                }
            }
        }

        private static void AssertSaneValue(ManageableYieldCurveSnapshot yieldCurveSnapshot)
        {
            Assert.NotNull(yieldCurveSnapshot);
            Assert.InRange(yieldCurveSnapshot.Values.Values.Count(), 2,200);

            foreach (var valueSnapshot in yieldCurveSnapshot.Values)
            {
                foreach (var snapshot in valueSnapshot.Value)
                {
                    ValueAssertions.AssertSensibleValue(snapshot.Value.MarketValue);
                    Assert.Null(snapshot.Value.OverrideValue);
                }

            }
        }

    }
}