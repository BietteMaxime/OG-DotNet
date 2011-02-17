using System;
using System.Linq;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Model.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

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
                Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument));

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
        public void CanAddAndGetRegions()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;

                foreach (Identifier region  in new[]{null, new Identifier("XX","12"),new Identifier("asd","asd") })
                {
                    YieldCurveDefinitionDocument yieldCurveDefinitionDocument = GenerateDocument();

                    yieldCurveDefinitionDocument.Definition.Region = region;
                    AssertRoundTrip(interpolatedYieldCurveDefinitionMaster, yieldCurveDefinitionDocument);
                }
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

        private static YieldCurveDefinitionDocument GenerateDocument()
        {
            string curveName = "My very special curve" + Guid.NewGuid();

            var yieldCurveDefinition = new YieldCurveDefinition(Currency.GetInstance("USD"), curveName, "dunno");
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip {ConventionName = "Somthing", CurveNodePointTime = Tenor.Day, InstrumentType = StripInstrumentType.CASH});
            yieldCurveDefinition.AddStrip(new FixedIncomeStrip { ConventionName = "Somthing", CurveNodePointTime = Tenor.Day, InstrumentType = StripInstrumentType.FUTURE, NthFutureFromTenor = 23});
            return new YieldCurveDefinitionDocument
                       {
                           Definition = yieldCurveDefinition
                       };
        }
    }
}
