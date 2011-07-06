//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceData.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.Time;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface
{
    //Just here for fudge
    [FudgeSurrogate(typeof(VolatilitySurfaceDataBuilder))]
    public class VolatilitySurfaceData
    {
    }

    public class VolatilitySurfaceData<TX, TY>
    {
        private readonly string _definitionName;
        private readonly string _specificationName;
        private readonly Currency _currency;
        private readonly string _interpolatorName;
        private readonly Dictionary<Tuple<TX, TY>, double> _values;
        private readonly IList<TX> _xs;
        private readonly IList<TY> _ys;

        public VolatilitySurfaceData(string definitionName, string specificationName, Currency currency, string interpolatorName, IList<TX> xs, IList<TY> ys, Dictionary<Tuple<TX, TY>, double> values)
        {
            if (values.Count != xs.Count * ys.Count)
            {
                throw new ArgumentException("Values not provided for all points");
            }

            _definitionName = definitionName;
            _specificationName = specificationName;
            _currency = currency;
            _interpolatorName = interpolatorName;
            _xs = xs;
            _ys = ys;
            _values = values;
        }

        public double this[TX x, TY y]
        {
            get
            {
                var tuple = new Tuple<TX, TY>(x, y);
                return _values[tuple];
            }
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public string DefinitionName
        {
            get { return _definitionName; }
        }

        public string SpecificationName
        {
            get { return _specificationName; }
        }

        public string InterpolatorName
        {
            get { return _interpolatorName; }
        }

        public IList<TX> Xs
        {
            get { return _xs; }
        }

        public IList<TY> Ys
        {
            get { return _ys; }
        }
    }

    public static class VolatilitySurfaceDataExtensions
    {
        public static Curve GetXSlice(this VolatilitySurfaceData<Tenor, Tenor> surface, Tenor x)
        {
            return new InterpolatedDoublesCurve(string.Format("Expiry {0}", x),
                                                surface.Ys.Select(t => t.TimeSpan.TotalMilliseconds).ToArray(),
                                                surface.Ys.Select(y => surface[x, y]).ToArray()
                );
        }

        public static Curve GetYSlice(this VolatilitySurfaceData<Tenor, Tenor> surface, Tenor y)
        {
            return new InterpolatedDoublesCurve(string.Format("Swap length {0}", y),
                                                surface.Xs.Select(t => t.TimeSpan.TotalMilliseconds).ToArray(),
                                                surface.Xs.Select(x => surface[x, y]).ToArray()
                );
        }
    }
}