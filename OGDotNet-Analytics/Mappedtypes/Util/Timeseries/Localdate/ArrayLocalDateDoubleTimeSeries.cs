using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.util.timeseries.fast.integer;

namespace OGDotNet.Mappedtypes.Util.Timeseries.Localdate
{
    class ArrayLocalDateDoubleTimeSeries : ILocalDateDoubleTimeSeries
    {
        private readonly LocalDateEpochDaysConverter _dateTimeConverter;
        private readonly FastArrayIntDoubleTimeSeries _fastTimeSeries;

        private ArrayLocalDateDoubleTimeSeries(LocalDateEpochDaysConverter dateTimeConverter, FastArrayIntDoubleTimeSeries fastTimeSeries)
        {
            _dateTimeConverter = dateTimeConverter;
            _fastTimeSeries = fastTimeSeries;
        }

        public LocalDateEpochDaysConverter DateTimeConverter
        {
            get { return _dateTimeConverter; }
        }

        public IList<Tuple<DateTimeOffset, double>> Values
        {
            get 
            {
                return _fastTimeSeries.Values.Select(t => new Tuple<DateTimeOffset, double>(_dateTimeConverter.Convert(t.Item1), t.Item2)).ToList();
            }
        }

        public static ArrayLocalDateDoubleTimeSeries FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var dateTimeConverterField = ffc.GetByOrdinal(1);
            var fastTimeSeriesField = ffc.GetByOrdinal(2);

            var dateTimeConverter = deserializer.FromField<LocalDateEpochDaysConverter>(dateTimeConverterField);
            var fastTimeSeries = deserializer.FromField<FastArrayIntDoubleTimeSeries>(fastTimeSeriesField);
            return new ArrayLocalDateDoubleTimeSeries(dateTimeConverter, fastTimeSeries);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
