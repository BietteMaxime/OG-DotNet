using System;
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
            var interpolatedYieldCurveSpecification = remoteInterpolatedYieldCurveSpecificationBuilder.BuildCurve(DateTimeOffset.Now, yieldCurveDefinitionDocument.Definition);
            Assert.NotNull(interpolatedYieldCurveSpecification);
        }
    }
}
