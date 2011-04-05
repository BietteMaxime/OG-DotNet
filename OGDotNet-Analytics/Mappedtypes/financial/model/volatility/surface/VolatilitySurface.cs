using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.math.surface;

namespace OGDotNet.Mappedtypes.financial.model.volatility.surface
{
    public class VolatilitySurface
    {
        /// <summary>
        /// TODO this could be any sort of surface
        /// </summary>
        private readonly ConstantDoublesSurface _sigma;

        public VolatilitySurface(ConstantDoublesSurface sigma)
        {
            _sigma = sigma;
        }

        public ConstantDoublesSurface Sigma
        {
            get { return _sigma; }
        }
    }


}