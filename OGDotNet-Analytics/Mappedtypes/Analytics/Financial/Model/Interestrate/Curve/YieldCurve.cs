//-----------------------------------------------------------------------
// <copyright file="YieldCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Analytics.Financial.Model.Interestrate.Curve
{
    public class YieldCurve
    {
        private readonly Math.Curve.Curve _curve;
        public Math.Curve.Curve Curve
        {
            get { return _curve; }
        }

        public YieldCurve(Math.Curve.Curve curve)
        {
            _curve = curve;
        }

        public double GetInterestRate(double t)
        {
            return Curve.GetYValue(t);
        }

        public double GetDiscountFactor(double t)
        {
            return System.Math.Exp(-t * GetInterestRate(t));
        }
    }
}