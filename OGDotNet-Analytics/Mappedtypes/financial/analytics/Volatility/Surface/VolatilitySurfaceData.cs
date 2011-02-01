using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Mappedtypes.Util.Time;

namespace OGDotNet_Analytics.Mappedtypes.financial.analytics.Volatility.Surface
{
    public class VolatilitySurfaceData
    {
        private String _definitionName;
        private String _specificationName;
        private string _currency;//TODO type
        private String _interpolatorName;
        private readonly Dictionary<Tuple<Tenor, Tenor>, double> _values;
        private IList<Tenor> _xs;
        private IList<Tenor> _ys;

        private VolatilitySurfaceData(string definitionName, string specificationName, string currency, string interpolatorName, IList<Tenor> xs, IList<Tenor> ys, Dictionary<Tuple<Tenor, Tenor>, double> values)
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

        public IEnumerable<Tenor> Xs
        {
            get { return _xs; }
        }

        public IEnumerable<Tenor> Ys
        {
            get { return _ys; }
        }

        public static VolatilitySurfaceData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string currency = ffc.GetValue<string>("currency");
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