using System;
using OGDotNet.Mappedtypes.math.curve;

namespace OGDotNet.Mappedtypes.financial.model.interestrate.curve
{
    public class YieldCurve
    {
        public Curve Curve { get; set; }


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