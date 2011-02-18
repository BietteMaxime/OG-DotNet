using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.util.timeseries.fast.integer
{
    public class FastArrayIntDoubleTimeSeries
    {
        private readonly DateTimeNumericEncoding _encoding;
        private readonly int[] _times;
        private readonly double[] _values;

        private FastArrayIntDoubleTimeSeries(DateTimeNumericEncoding encoding, int[] times, double[] values)
        {
            if (times.Length!= values.Length)
                throw new ArgumentException("Graph is not square");
            _encoding = encoding;
            _times = times;
            _values = values;
        }

        public IList<Tuple<DateTime,double>> Values{
            get
            {
                return _times.Select(t => _encoding.ConvertToDateTime(t)).Zip(_values, (t, v) => new Tuple<DateTime, double>(t, v)).ToList();
            }
        }
        public static FastArrayIntDoubleTimeSeries FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var encoding = deserializer.FromField<DateTimeNumericEncoding>(ffc.GetByOrdinal(1));
            var times = ffc.GetValue<int[]>(2);
            var values= ffc.GetValue<double[]>(3);

            return new FastArrayIntDoubleTimeSeries(encoding,times,values);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }

    
}