// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveDefinitionMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using OpenGamma.Financial.Analytics.IRCurve;
using OpenGamma.Financial.User;
using OpenGamma.Id;
using OpenGamma.Util.Money;
using OpenGamma.Util.Time;
using OpenGamma.Xunit.Extensions;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class InterpolatedYieldCurveDefinitionMasterTests : RemoteEngineContextTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.NotNull(interpolatedYieldCurveDefinitionMaster);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAdd()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                var newDoc = interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);

                Assert.True(ReferenceEquals(newDoc, yieldCurveDefinitionDocument));
                Assert.True(ReferenceEquals(newDoc.YieldCurveDefinition, yieldCurveDefinitionDocument.YieldCurveDefinition));
                Assert.NotNull(yieldCurveDefinitionDocument.UniqueId);
            }
        }

        [Xunit.Extensions.Fact]
        public void CantAddTwice()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                var exception = Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument));
                Assert.True(exception.Message.Contains("Duplicate definition"));
            }
        }

        [Xunit.Extensions.Fact]
        public void CantAddOrUpdate()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }

        [Xunit.Extensions.Fact]
        public void CantAddOrUpdateAfterAdd()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndGet()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;

                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Xunit.Extensions.Fact]
        public void CantGetMissing()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.Throws<DataNotFoundException>(() => interpolatedYieldCurveDefinitionMaster.Get(UniqueId.Create("InMemoryInterpolatedYieldCurveDefinition", long.MaxValue.ToString())));
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndGetRegions()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;

                foreach (ExternalId region in new[] { null, new ExternalId("XX", "12"), new ExternalId("asd", "asd") })
                {
                    YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                    yieldCurveDefinitionDocument.YieldCurveDefinition.Region = region;
                    AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndGetAllInstrumentTypes()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster =
                    financialClient.InterpolatedYieldCurveDefinitionMaster;
                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                YieldCurveDefinition yieldCurveDefinition = yieldCurveDefinitionDocument.YieldCurveDefinition;

                foreach (StripInstrumentType stripInstrumentType in new[]
                    {
                        StripInstrumentType.Libor, StripInstrumentType.Cash, StripInstrumentType.BankersAcceptance,
                        StripInstrumentType.Euribor, StripInstrumentType.Cdor, StripInstrumentType.Cibor, StripInstrumentType.Stibor
                    })
                {
                    var fixedIncomeStrip = new FixedIncomeStrip
                        {
                            InstrumentType = stripInstrumentType,
                            ConventionName = "DEFAULT",
                            CurveNodePointTime = Tenor.Day,
                        };
                    yieldCurveDefinition.AddStrip(fixedIncomeStrip);
                }

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                    {
                        InstrumentType = StripInstrumentType.Future,
                        ConventionName = "DEFAULT",
                        CurveNodePointTime = Tenor.Year,
                        NthFutureFromTenor = 3
                    });

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                    {
                        InstrumentType = StripInstrumentType.Fra,
                        ConventionName = "DEFAULT",
                        CurveNodePointTime = Tenor.SixMonths
                    });

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                {
                    InstrumentType = StripInstrumentType.Fra3M,
                    ConventionName = "DEFAULT",
                    CurveNodePointTime = Tenor.SixMonths
                });

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                {
                    InstrumentType = StripInstrumentType.Fra6M,
                    ConventionName = "DEFAULT",
                    CurveNodePointTime = Tenor.NineMonths
                });

                foreach (StripInstrumentType stripInstrumentType in new[]
                    {
                        StripInstrumentType.Swap, StripInstrumentType.Swap3M, StripInstrumentType.Swap6M,
                        StripInstrumentType.Swap12M, StripInstrumentType.TenorSwap
                    })
                {
                    yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                    {
                        InstrumentType = stripInstrumentType,
                        ConventionName = "DEFAULT",
                        CurveNodePointTime = Tenor.ThreeYears
                    });
                }

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                {
                    InstrumentType = StripInstrumentType.OisSwap,
                    ConventionName = "DEFAULT",
                    CurveNodePointTime = Tenor.ThreeYears,
                    ResetTenor = Tenor.ThreeMonths,
                    IndexType = IndexType.Libor
                });

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                {
                    InstrumentType = StripInstrumentType.BasisSwap,
                    ConventionName = "DEFAULT",
                    CurveNodePointTime = Tenor.ThreeYears,
                    PayTenor = Tenor.ThreeMonths,
                    ReceiveTenor = Tenor.SixMonths,
                    PayIndexType = IndexType.Libor,
                    ReceiveIndexType = IndexType.Libor
                });

                yieldCurveDefinition.AddStrip(new FixedIncomeStrip
                {
                    InstrumentType = StripInstrumentType.PeriodicZeroDeposit,
                    ConventionName = "DEFAULT",
                    CurveNodePointTime = Tenor.Year,
                    PeriodsPerYear = 4
                });

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAddAndRemove()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;

                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
                interpolatedYieldCurveDefinitionMaster.Remove(yieldCurveDefinitionDocument.UniqueId);

                Assert.Throws<DataNotFoundException>(() => interpolatedYieldCurveDefinitionMaster.Get(yieldCurveDefinitionDocument.UniqueId));
            }
        }

        private static void AssertRoundTrip(InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster, YieldCurveDefinitionDocument yieldCurveDefinitionDocument)
        {
            interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);

            YieldCurveDefinitionDocument roundtrippedDoc = interpolatedYieldCurveDefinitionMaster.Get(yieldCurveDefinitionDocument.UniqueId);

            YieldCurveDefinition roundTripped = roundtrippedDoc.YieldCurveDefinition;

            var yieldCurveDefinition = yieldCurveDefinitionDocument.YieldCurveDefinition;
            Assert.Equal(yieldCurveDefinition.Name, roundTripped.Name);
            Assert.Equal(yieldCurveDefinition.InterpolatorName, roundTripped.InterpolatorName);
            Assert.Equal(yieldCurveDefinition.Currency, roundTripped.Currency);
            Assert.Equal(roundTripped.Region, roundTripped.Region);

            Assert.True(roundTripped.Strips.SequenceEqual(roundTripped.Strips));
        }

        public static YieldCurveDefinitionDocument GenerateDocument()
        {
            string curveName = TestUtils.GetUniqueName();

            var yieldCurveDefinition = new YieldCurveDefinition(Currency.USD, curveName, "Linear") { Region = new ExternalId("SOMEWHERE", "Europe") };
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip { ConventionName = "DEFAULT", CurveNodePointTime = Tenor.Day, InstrumentType = StripInstrumentType.Cash });
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip { ConventionName = "DEFAULT", CurveNodePointTime = Tenor.TwoYears, InstrumentType = StripInstrumentType.Future, NthFutureFromTenor = 23 });
            return new YieldCurveDefinitionDocument
                       {
                           YieldCurveDefinition = yieldCurveDefinition
                       };
        }
    }
}
