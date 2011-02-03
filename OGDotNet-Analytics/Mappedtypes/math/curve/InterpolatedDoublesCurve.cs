using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Mappedtypes.math.curve
{
    public class InterpolatedDoublesCurve
    {
        private readonly double[] _xData;
        private readonly double[] _yData;

        private InterpolatedDoublesCurve(double[] xData, double[] yData)
        {
            if (xData.Length != yData.Length)
            {
                throw new ArgumentException("Graph is not square");
            }

            _xData = xData;
            _yData = yData;
        }

        public IList<double> XData
        {
            get { return _xData; }
        }
        public IList<double> YData
        {
            get { return _yData; }
        }

        public IEnumerable<Tuple<double,double>> Data
        {
            get
            {
                return _xData.Zip(_yData, (x,y) => new Tuple<double,double>(x,y));
            }
        }
        
        public static InterpolatedDoublesCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var xData = ffc.GetValue<double[]>("x data");
            var yData = ffc.GetValue<double[]>("y data");

            return new InterpolatedDoublesCurve(xData, yData);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}