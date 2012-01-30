//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveSpecificationBuilderTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Assert = global::Xunit.Assert;

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
            var reqDef = yieldCurveDefinitionDocument.YieldCurveDefinition;

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
                    s => fixedIncomeStrip.CurveNodePointTime == s.Strip.CurveNodePointTime && s.Strip.InstrumentType == fixedIncomeStrip.InstrumentType
                    ).ToList();
                Assert.Single(matches);
                var fixedIncomeStripWithIdentifier = matches.First();
                Assert.NotNull(fixedIncomeStripWithIdentifier.Security);

                var security = Context.SecuritySource.GetSecurity(new ExternalIdBundle(fixedIncomeStripWithIdentifier.Security));
                if (fixedIncomeStrip.InstrumentType == StripInstrumentType.Future)
                {
                    Assert.Equal(EnumBuilder<StripInstrumentType>.GetJavaName(fixedIncomeStripWithIdentifier.Strip.InstrumentType), security.SecurityType);
                }
                else
                {
                    Assert.Null(security);
                }
            }
        }
    }
}
