using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.math.curve
{
    public class InterpolatedDoublesCurve : Curve
    {
        private readonly IList<double> _xData;
        private readonly IList<double> _yData;

        public InterpolatedDoublesCurve(string name, IList<double> xData, IList<double> yData)
            : base(name)
        {
            if (xData.Count != yData.Count)
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
                return _xData.Count;
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