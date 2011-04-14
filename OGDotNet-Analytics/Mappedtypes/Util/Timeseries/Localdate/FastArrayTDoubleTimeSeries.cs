//-----------------------------------------------------------------------
// <copyright file="FastArrayTDoubleTimeSeries.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.util.timeseries.fast.integer
{
    public class FastArrayTDoubleTimeSeries<T>
    {
        protected readonly DateTimeNumericEncoding Encoding;
        protected readonly T[] Times;
        protected readonly double[] _values;

        protected FastArrayTDoubleTimeSeries(DateTimeNumericEncoding encoding, T[] times, double[] values)
        {
            if (times.Length != values.Length)
                throw new ArgumentException("Graph is not square");
            Encoding = encoding;
            Times = times;
            _values = values;
        }

        protected static Tuple<DateTimeNumericEncoding, T[], double[]> FromFudgeMsgImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var encoding = deserializer.FromField<DateTimeNumericEncoding>(ffc.GetByOrdinal(1));
            var times = ffc.GetValue<T[]>(2);
            var values = ffc.GetValue<double[]>(3);

            return Tuple.Create(encoding, times, values);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}