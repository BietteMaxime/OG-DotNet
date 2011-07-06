//-----------------------------------------------------------------------
// <copyright file="VolatilitySurface.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.math.surface;

namespace OGDotNet.Mappedtypes.financial.model.volatility.surface
{
    public class VolatilitySurface
    {
        private readonly IDoublesSurface _sigma;

        public VolatilitySurface(IDoublesSurface sigma)
        {
            _sigma = sigma;
        }

        public IDoublesSurface Sigma
        {
            get { return _sigma; }
        }
    }
}