using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.util.timeseries.fast;
using OGDotNet.Mappedtypes.util.timeseries.fast.integer;

namespace OGDotNet.Mappedtypes.Util.Timeseries.fast.longint
{
    public class FastArrayLongDoubleTimeSeries : FastArrayTDoubleTimeSeries<long>
    {
        private FastArrayLongDoubleTimeSeries(DateTimeNumericEncoding encoding, long[] times, double[] values)
            : base(encoding, times, values)
        {
        }

        public IList<Tuple<DateTime,double>> Values{
            get
            {
                return Times.Select(t => Encoding.ConvertToDateTime(t)).Zip(_values, (t, v) => new Tuple<DateTime, double>(t, v)).ToList();
            }
        }

        public static FastArrayLongDoubleTimeSeries FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var bits = FromFudgeMsgImpl(ffc,deserializer);
            return new FastArrayLongDoubleTimeSeries(bits.Item1, bits.Item2, bits.Item3);
        }
    }
}
