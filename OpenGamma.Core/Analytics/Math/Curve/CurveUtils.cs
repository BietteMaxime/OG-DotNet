// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurveUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenGamma.Analytics.Math.Curve
{
    public static class CurveUtils
    {
        public static IEnumerable<Tuple<double, double>> GetData(this Curve c)
        {
            return c.XData.Zip(c.YData, (x, y) => new Tuple<double, double>(x, y)).ToList();
        }
    }
}