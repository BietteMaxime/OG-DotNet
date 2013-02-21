// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionalDoublesCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Analytics.Financial.InterestRate;

namespace OpenGamma.Analytics.Math.Curve
{
    public class FunctionalDoublesCurve : Analytics.Math.Curve.Curve
    {
        private readonly Func<double, double> _function;

        private FunctionalDoublesCurve(Func<double, double> function, string name) : base(name)
        {
            _function = function;
        }

        public static FunctionalDoublesCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string name = GetName(ffc);
            double[] parameters = ffc.GetValue<double[]>("NSS parameters");
            return Create(new NelsonSiegelSvennsonBondCurveModel(parameters).Eval, name);
        }

        public override IList<double> XData
        {
            get
            {
                throw new InvalidOperationException("Cannot get x data - this curve is defined by a function (x -> y)");
            }
        }

        public override IList<double> YData
        {
            get
            {
                throw new InvalidOperationException("Cannot get y data - this curve is defined by a function (x -> y)");
            }
        }

        public override bool IsVirtual
        {
            get { return true; }
        }

        public override double GetYValue(double x)
        {
            return _function(x);
        }

        public static FunctionalDoublesCurve Create(Func<double, double> function, string name)
        {
            return new FunctionalDoublesCurve(function, name);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
