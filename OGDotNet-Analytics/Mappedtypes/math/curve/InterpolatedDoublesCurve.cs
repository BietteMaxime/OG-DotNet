using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.math.curve
{
    public class InterpolatedDoublesCurve : Curve
    {
        private readonly double[] _xData;
        private readonly double[] _yData;

        private InterpolatedDoublesCurve(string name, double[] xData, double[] yData) : base(name)
        {
            if (xData.Length != yData.Length)
            {
                throw new ArgumentException("Graph is not square");
            }

            _xData = xData;
            _yData = yData;
        }

        public override IList<double> XData
        {
            get { return _xData; }
        }
        public override IList<double> YData
        {
            get { return _yData; }
        }
        public int Size
        {
            get
            {
                return _xData.Length;
            }
        }

        public override double GetYValue(double x)
        {
            throw new NotImplementedException();
        }
        
        public static InterpolatedDoublesCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string name = GetName(ffc);
            var xData = ffc.GetValue<double[]>("x data");
            var yData = ffc.GetValue<double[]>("y data");

            return new InterpolatedDoublesCurve(name, xData, yData);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}