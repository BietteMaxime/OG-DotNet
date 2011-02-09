using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.financial.model.interestrate;

namespace OGDotNet.Mappedtypes.math.curve
{
    //TODO
    public class FunctionalDoublesCurve : Curve
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
            return From(new NelsonSiegelSvennsonBondCurveModel(parameters).Eval, name);
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


        public override double GetYValue(double x)
        {
            return _function(x);
        }

        public static FunctionalDoublesCurve From(Func<double,double> function, string name)
        {
            return new FunctionalDoublesCurve(function, name);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

    }
}
