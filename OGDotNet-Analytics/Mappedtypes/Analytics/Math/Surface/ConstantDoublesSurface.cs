//-----------------------------------------------------------------------
// <copyright file="ConstantDoublesSurface.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Analytics.Math.Surface
{
    public class ConstantDoublesSurface : IDoublesSurface
    {
        private readonly double _z;
        private readonly string _name;

        private ConstantDoublesSurface(double z, string name)
        {
            _z = z;
            _name = name;
        }

        public double Z
        {
            get { return _z; }
        }

        public string Name
        {
            get { return _name; }
        }

        public double[] XData
        {
            get { throw new NotSupportedException(); }
        }

        public double[] YData
        {
            get { throw new NotSupportedException(); }
        }

        public double[] ZData
        {
            get { throw new NotSupportedException(); }
        }

        public double GetZValue(double x, double y)
        {
            return Z;
        }

        public static ConstantDoublesSurface FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            double z = ffc.GetDouble("z value").Value;
            string name = ffc.GetString("surface name");
            return new ConstantDoublesSurface(z, name);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}