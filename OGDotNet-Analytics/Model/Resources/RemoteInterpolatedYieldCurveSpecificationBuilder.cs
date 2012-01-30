//-----------------------------------------------------------------------
// <copyright file="RemoteInterpolatedYieldCurveSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.Financial.Analytics.IRCurve;

namespace OGDotNet.Model.Resources
{
    public class RemoteInterpolatedYieldCurveSpecificationBuilder   
    {
        private readonly RestTarget _rest;

        public RemoteInterpolatedYieldCurveSpecificationBuilder(RestTarget rest)
        {
            _rest = rest;
        }

        public InterpolatedYieldCurveSpecification BuildCurve(DateTimeOffset curveDate, YieldCurveDefinition curveDefinition)
        {
            RestTarget target = _rest.Resolve("builder").Resolve(UriEncoding.ToString(curveDate));
            return target.Post<InterpolatedYieldCurveSpecification>(curveDefinition);
        }
    }
}