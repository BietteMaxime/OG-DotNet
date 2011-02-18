using System;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;

namespace OGDotNet.Model.Resources
{
    
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
            RestTarget target = _rest.Resolve(curveDate.ToString("yyyy-MM-dd"));
            return  target.Post<InterpolatedYieldCurveSpecification>(new YieldCurveDefinitionDocument{Definition =  curveDefinition}, "specification");
        }
    }
}