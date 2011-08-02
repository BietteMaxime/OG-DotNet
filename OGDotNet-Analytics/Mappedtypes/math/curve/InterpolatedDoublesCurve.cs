//-----------------------------------------------------------------------
// <copyright file="InterpolatedDoublesCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Math.Curve
{
    public class InterpolatedDoublesCurve : Curve
    {
        private readonly double[] _xData;
        private readonly double[] _yData;

        public InterpolatedDoublesCurve(string name, double[] xData, double[] yData)
            : base(name)
        {
            ArgumentChecker.NotEmpty(xData, "xData");
            ArgumentChecker.NotEmpty(yData, "yData");
            ArgumentChecker.Not(xData.Length != yData.Length, "Graph is not square");

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

        public override bool IsVirtual
        {
            get { return false; }
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
            var index = Array.BinarySearch(_xData, x); // Using equals on double is ridiculous, this only works if they've read our x value out
            if (index < 0 )
            {
                throw new NotImplementedException("I'm not sure if the interpolation should happen client side");
            }

            return YData[index];
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