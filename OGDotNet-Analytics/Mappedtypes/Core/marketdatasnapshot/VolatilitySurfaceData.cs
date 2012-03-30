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
using OGDotNet.Mappedtypes.Analytics.Math.Curve;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot
{
    //Mainly here for fudge
    [FudgeSurrogate(typeof(VolatilitySurfaceDataBuilder))]
    public class VolatilitySurfaceData
    {
        private readonly string _definitionName;
        private readonly string _specificationName;
        private readonly Currency _currency;
        private readonly string _interpolatorName;

        public VolatilitySurfaceData(string definitionName, string specificationName, Currency currency, string interpolatorName)
        {
            _definitionName = definitionName;
            _specificationName = specificationName;
            _currency = currency;
            _interpolatorName = interpolatorName;
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
    }

    public class VolatilitySurfaceData<TX, TY> : VolatilitySurfaceData
    {
        private readonly Dictionary<Tuple<TX, TY>, double> _values;
        private readonly IList<TX> _xs;
        private readonly IList<TY> _ys;

        public VolatilitySurfaceData(string definitionName, string specificationName, Currency currency, string interpolatorName, IList<TX> xs, IList<TY> ys, Dictionary<Tuple<TX, TY>, double> values)
            : base(definitionName, specificationName, currency, interpolatorName)
        {
            _xs = xs;
            _ys = ys;
            _values = values;
        }

        /// <summary>
        /// Note that VolatilitySurfaceData can be sparse
        /// </summary>
        public double this[TX x, TY y]
        {
            get
            {
                var tuple = new Tuple<TX, TY>(x, y);
                return _values[tuple];
            }
        }

        public bool TryGet(TX x, TY y, out double value)
        {
            var tuple = new Tuple<TX, TY>(x, y);
            return _values.TryGetValue(tuple, out value);
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
            var values = surface.Xs.Select(
                delegate(Tenor t) {
                                      double value;
                                      var have = surface.TryGet(t, y, out value);
                                      return Tuple.Create(t.TimeSpan.TotalMilliseconds, have, value);
                }).Where(t => t.Item2);

            return new InterpolatedDoublesCurve(string.Format("Swap length {0}", y),
                                                values.Select(t => t.Item1).ToArray(),
                                                values.Select(t => t.Item3).ToArray()
                );
        }
    }
}