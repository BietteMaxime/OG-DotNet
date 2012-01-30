//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveDefinitionMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Financial.User;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class InterpolatedYieldCurveDefinitionMasterTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.NotNull(interpolatedYieldCurveDefinitionMaster);
            }
        }

        [Fact]
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
        [Fact]
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

        [Fact]
        public void CantAddOrUpdate()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }
        [Fact]
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

        [Fact]
        public void CanAddAndGet()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;

                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Fact]
        public void CantGetMissing()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = financialClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.Throws<DataNotFoundException>(() => interpolatedYieldCurveDefinitionMaster.Get(UniqueId.Create("InMemoryInterpolatedYieldCurveDefinition", long.MaxValue.ToString())));
            }
        }

        [Fact]
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

        [Fact]
        public void CanAddAndGetAllInstrumentTypes()
        {
            using (FinancialClient financialClient = Context.CreateFinancialClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster =
                    financialClient.InterpolatedYieldCurveDefinitionMaster;
                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                foreach (StripInstrumentType stripInstrumentType in Enum.GetValues(typeof(StripInstrumentType)))
                {
                    var fixedIncomeStrip = new FixedIncomeStrip
                                            {
                                                ConventionName = "DEFAULT",
                                                CurveNodePointTime = Tenor.Day,
                                                InstrumentType = stripInstrumentType
                                            };
                    if (stripInstrumentType == StripInstrumentType.Future)
                        fixedIncomeStrip.NthFutureFromTenor = 12;

                    yieldCurveDefinitionDocument.YieldCurveDefinition.AddStrip(fixedIncomeStrip);
                }

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Fact]
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
