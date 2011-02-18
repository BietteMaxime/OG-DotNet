using System;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;

namespace OGDotNet.Model.Resources
{
    public static class UriEncoding
    {
        public static string ToString(DateTimeOffset curveDate)
        {
            return curveDate.ToString("yyyy-MM-dd");
        }
    }
    public class RemoteInterpolatedYieldCurveSpecificationBuilder   
    {
        private readonly OpenGammaFudgeContext _context;
        private readonly RestTarget _rest;

        public RemoteInterpolatedYieldCurveSpecificationBuilder(OpenGammaFudgeContext context, RestTarget rest)
        {
            _context = context;
            _rest = rest;
        }

        public InterpolatedYieldCurveSpecification BuildCurve(DateTimeOffset curveDate, YieldCurveDefinition curveDefinition)
        {
            RestTarget target = _rest.Resolve(UriEncoding.ToString(curveDate));
            return  target.Post<InterpolatedYieldCurveSpecification>(new YieldCurveDefinitionDocument{Definition =  curveDefinition}, "specification");
        }
    }
}