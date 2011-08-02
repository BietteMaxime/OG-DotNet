//-----------------------------------------------------------------------
// <copyright file="InterpolatedDoublesSurface.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Math.Surface
{
    public class InterpolatedDoublesSurface : IDoublesSurface
    {
        private readonly double[] _xData;
        private readonly double[] _yData;
        private readonly double[] _zData;

        public InterpolatedDoublesSurface(double[] xData, double[] yData, double[] zData)
        {
            if (xData.Length != yData.Length || xData.Length != zData.Length)
            {
                throw new ArgumentException("Data arrays must be of the same size");
            }
            _xData = xData;
            _yData = yData;
            _zData = zData;
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public double[] XData
        {
            get { return _xData; }
        }

        public double[] YData
        {
            get { return _yData; }
        }

        public double[] ZData
        {
            get { return _zData; }
        }

        public double GetZValue(double x, double y)
        {
            throw new NotImplementedException();
        }

        public static InterpolatedDoublesSurface FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var xData = ffc.GetValue<double[]>("x data");
            var yData = ffc.GetValue<double[]>("y data");
            var zData = ffc.GetValue<double[]>("z data");
            return new InterpolatedDoublesSurface(xData, yData, zData);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
