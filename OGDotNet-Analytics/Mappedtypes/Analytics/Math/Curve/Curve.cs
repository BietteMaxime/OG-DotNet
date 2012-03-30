//-----------------------------------------------------------------------
// <copyright file="Curve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Fudge;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Analytics.Math.Curve
{
    public abstract class Curve
    {
        private readonly string _name;

        protected Curve(string name)
        {
            ArgumentChecker.NotNull(name, "name");
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public abstract IList<double> XData { get; }
        public abstract IList<double> YData { get; }

        public abstract bool IsVirtual { get; }

        public abstract double GetYValue(double x);

        protected static string GetName(IFudgeFieldContainer ffc)
        {
            return ffc.GetString("name") ?? ffc.GetString("curve name");
        }
    }
}