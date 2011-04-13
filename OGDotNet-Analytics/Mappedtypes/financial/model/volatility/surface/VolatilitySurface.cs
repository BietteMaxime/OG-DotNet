//-----------------------------------------------------------------------
// <copyright file="VolatilitySurface.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

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