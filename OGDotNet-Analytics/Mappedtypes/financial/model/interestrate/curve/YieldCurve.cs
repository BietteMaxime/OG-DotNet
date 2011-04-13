//-----------------------------------------------------------------------
// <copyright file="YieldCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.math.curve;

namespace OGDotNet.Mappedtypes.financial.model.interestrate.curve
{
    public class YieldCurve
    {
        private readonly Curve _curve;
        public Curve Curve
        {
            get { return _curve; }
        }

        public YieldCurve(Curve curve)
        {
            _curve = curve;
        }


        public double GetInterestRate(double t)
        {
            return Curve.GetYValue(t);
        }

        public double GetDiscountFactor(double t)
        {
            return Math.Exp(-t * GetInterestRate(t));
        }
    }
}