using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Assert=global::Xunit.Assert;
namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class InterpolatedYieldCurveSpecificationBuilderTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            var remoteInterpolatedYieldCurveSpecificationBuilder = Context.InterpolatedYieldCurveSpecificationBuilder;
            Assert.NotNull(remoteInterpolatedYieldCurveSpecificationBuilder);
        }
        [Fact]
        public void CanBuild()
        {
            var remoteInterpolatedYieldCurveSpecificationBuilder = Context.InterpolatedYieldCurveSpecificationBuilder;
            var yieldCurveDefinitionDocument = InterpolatedYieldCurveDefinitionMasterTests.GenerateDocument();
            var reqDef = yieldCurveDefinitionDocument.Definition;

            var reqDate = DateTimeOffset.Now.Date;

            var interpolatedYieldCurveSpecification = remoteInterpolatedYieldCurveSpecificationBuilder.BuildCurve(reqDate, reqDef);
            Assert.NotNull(interpolatedYieldCurveSpecification);

            Assert.Equal(reqDef.Currency, interpolatedYieldCurveSpecification.Currency);
            Assert.Equal(reqDate.Date, interpolatedYieldCurveSpecification.CurveDate.Date);
            Assert.Equal(reqDef.Name, interpolatedYieldCurveSpecification.Name);
            Assert.Equal(reqDef.Region, interpolatedYieldCurveSpecification.Region);

            Assert.Equal(reqDef.Strips.Count, interpolatedYieldCurveSpecification.ResolvedStrips.Count);

            foreach (var fixedIncomeStrip in reqDef.Strips)
            {
                var matches = interpolatedYieldCurveSpecification.ResolvedStrips.Where(
                    s=>fixedIncomeStrip.CurveNodePointTime == s.Maturity && s.InstrumentType == fixedIncomeStrip.InstrumentType
                    ).ToList();
                Assert.Single(matches);
                var fixedIncomeStripWithIdentifier = matches.First();
                Assert.NotNull(fixedIncomeStripWithIdentifier.Security);

                var security = Context.SecuritySource.GetSecurity(new IdentifierBundle(fixedIncomeStripWithIdentifier.Security));
                if (fixedIncomeStrip.InstrumentType == StripInstrumentType.FUTURE)
                {
                    Assert.Equal(fixedIncomeStrip.InstrumentType.ToString(), security.SecurityType);
                }
                else
                {
                    Assert.Null(security);
                }
                
            }
        }
    }
}
