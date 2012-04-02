//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Surface
{
    public class VolatilitySurfaceSpecification
    {
        private readonly IUniqueIdentifiable _target;

        public VolatilitySurfaceSpecification(IUniqueIdentifiable target)
        {
            _target = target;
        }

        public IUniqueIdentifiable Target
        {
            get { return _target; }
        }
    }
}
