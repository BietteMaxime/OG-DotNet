//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveDefinitionMasterTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class InterpolatedYieldCurveDefinitionMasterTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.NotNull(interpolatedYieldCurveDefinitionMaster);
            }
        }

        [Fact]
        public void CanAdd()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                var newDoc = interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);

                Assert.True(ReferenceEquals(newDoc, yieldCurveDefinitionDocument));
                Assert.True(ReferenceEquals(newDoc.Definition, yieldCurveDefinitionDocument.Definition));
                Assert.NotNull(yieldCurveDefinitionDocument.UniqueId);
            }
        }
        [Fact]
        public void CantAddTwice()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                var exception = Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument));
                Assert.True(exception.Message.Contains("Duplicate definition"));
            }
        }

        [Fact]
        public void CantAddOrUpdate()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }
        [Fact]
        public void CantAddOrUpdateAfterAdd()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinitionDocument = GenerateDocument();
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }

        [Fact]
        public void CanAddAndGet()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;

                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Fact]
        public void CantGetMissing()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;

                var argumentException = Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Get(UniqueIdentifier.Of("XX", "Mising" + Guid.NewGuid())));
                Assert.True(argumentException.Message.StartsWith("Not found"));
            }
        }

        [Fact]
        public void CanAddAndGetRegions()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;

                foreach (Identifier region in new[] { null, new Identifier("XX", "12"), new Identifier("asd", "asd") })
                {
                    YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                    yieldCurveDefinitionDocument.Definition.Region = region;
                    AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
                }
            }
        }

        [Fact]
        public void CanAddAndGetAllInstrumentTypes()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster =
                    remoteClient.InterpolatedYieldCurveDefinitionMaster;
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

                    yieldCurveDefinitionDocument.Definition.AddStrip(fixedIncomeStrip);
                }

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
            }
        }

        [Fact]
        public void CanAddAndRemove()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;

                YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
                interpolatedYieldCurveDefinitionMaster.Remove(yieldCurveDefinitionDocument.UniqueId);

                var argumentException = Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Get(yieldCurveDefinitionDocument.UniqueId));
                Assert.True(argumentException.Message.StartsWith("Not found"));
            }
        }

        private static void AssertRoundTrip(InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster, YieldCurveDefinitionDocument yieldCurveDefinitionDocument)
        {
            interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);

            YieldCurveDefinitionDocument roundtrippedDoc = interpolatedYieldCurveDefinitionMaster.Get(yieldCurveDefinitionDocument.UniqueId);

            YieldCurveDefinition roundTripped = roundtrippedDoc.Definition;

            var yieldCurveDefinition = yieldCurveDefinitionDocument.Definition;
            Assert.Equal(yieldCurveDefinition.Name, roundTripped.Name);
            Assert.Equal(yieldCurveDefinition.InterpolatorName, roundTripped.InterpolatorName);
            Assert.Equal(yieldCurveDefinition.Currency, roundTripped.Currency);
            Assert.Equal(roundTripped.Region, roundTripped.Region);

            Assert.True(roundTripped.Strips.SequenceEqual(roundTripped.Strips));
        }

        public static YieldCurveDefinitionDocument GenerateDocument()
        {
            string curveName = TestUtils.GetUniqueName();

            var yieldCurveDefinition = new YieldCurveDefinition(Currency.USD, curveName, "Linear") { Region = new Identifier("SOMEWHERE", "Europe") };
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip { ConventionName = "DEFAULT", CurveNodePointTime = Tenor.Day, InstrumentType = StripInstrumentType.Cash });
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip { ConventionName = "DEFAULT", CurveNodePointTime = Tenor.TwoYears, InstrumentType = StripInstrumentType.Future, NthFutureFromTenor = 23 });
            return new YieldCurveDefinitionDocument
                       {
                           Definition = yieldCurveDefinition
                       };
        }
    }
}
