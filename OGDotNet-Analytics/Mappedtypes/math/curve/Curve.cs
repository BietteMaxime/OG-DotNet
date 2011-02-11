﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;

namespace OGDotNet.Mappedtypes.math.curve
{
    public static class CurveExtensionMethods
    {
        public static IEnumerable<Tuple<double,double>> GetData(this Curve c)
        {
            return c.XData.Zip(c.YData, (x, y) => new Tuple<double, double>(x, y)).ToList();
        }
    }
    public abstract class Curve
    {
        private readonly string _name;

        protected Curve(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public abstract IList<double> XData { get; }
        public abstract IList<double> YData { get; }

        public abstract double GetYValue(double x);

        protected static string GetName(IFudgeFieldContainer ffc)
        {
            return ffc.GetValue<string>("curve name");
        }
    }
}