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
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Core.Common;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface
{
    public class VolatilitySurfaceData
    {
        private readonly String _definitionName;
        private readonly String _specificationName;
        private readonly Currency _currency;
        private readonly String _interpolatorName;
        private readonly Dictionary<Tuple<Tenor, Tenor>, double> _values;
        private readonly IList<Tenor> _xs;
        private readonly IList<Tenor> _ys;

        private VolatilitySurfaceData(string definitionName, string specificationName, Currency currency, string interpolatorName, IList<Tenor> xs, IList<Tenor> ys, Dictionary<Tuple<Tenor, Tenor>, double> values)
        {
            _definitionName = definitionName;
            _specificationName = specificationName;
            _currency = currency;
            _interpolatorName = interpolatorName;
            _xs = xs;
            _ys = ys;
            _values = values;
        }

        public double this[Tenor x, Tenor y]
        {
            get { return _values[new Tuple<Tenor, Tenor>(x, y)]; }
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

        public IList<Tenor> Xs
        {
            get { return _xs; }
        }

        public IList<Tenor> Ys
        {
            get { return _ys; }
        }

        public Curve GetXSlice(Tenor x)
        {
            return new InterpolatedDoublesCurve(string.Format("Expiry {0}", x),
                                                Ys.Select(t => t.TimeSpan.TotalMilliseconds).ToArray(),
                                                Ys.Select(y => this[x, y]).ToArray()
                );
        }

        public Curve GetYSlice(Tenor y)
        {
            return new InterpolatedDoublesCurve(string.Format("Swap length {0}", y),
                                                Xs.Select(t => t.TimeSpan.TotalMilliseconds).ToArray(),
                                                Xs.Select(x => this[x, y]).ToArray()
                );
        }

        public static VolatilitySurfaceData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string currencyName = ffc.GetValue<string>("currency");
            Currency currency = Currency.Create(currencyName);
            string definitionName = ffc.GetValue<String>("definitionName");
            string specificationName = ffc.GetValue<String>("specificationName");
            string interpolatorName = ffc.GetValue<String>("interpolatorName");


            IList<Tenor> xs = ffc.GetAllByName("xs").Select(deserializer.FromField<Tenor>).ToList();
            IList<Tenor> ys = ffc.GetAllByName("ys").Select(deserializer.FromField<Tenor>).ToList();

            var values = new Dictionary<Tuple<Tenor, Tenor>, double>();
            var valuesFields = ffc.GetAllByName("values");
            foreach (var valueField in valuesFields)
            {
                var subMessage = (IFudgeFieldContainer)valueField.Value;
                Tenor x = deserializer.FromField<Tenor>(subMessage.GetByName("x"));
                Tenor y = deserializer.FromField<Tenor>(subMessage.GetByName("y"));
                double value = subMessage.GetValue<double>("value");
                values.Add(new Tuple<Tenor, Tenor>(x, y), value);
            }
            return new VolatilitySurfaceData(definitionName, specificationName, currency, interpolatorName, xs, ys, values);

        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

       
    }
}