// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastArrayTDoubleTimeSeries.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Util.TimeSeries.Fast;

namespace OpenGamma.Util.TimeSeries
{
    public abstract class FastArrayTDoubleTimeSeries<T>
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

        public abstract IList<Tuple<DateTime, double>> Values { get; }

        protected static Tuple<DateTimeNumericEncoding, T[], double[]> FromFudgeMsgImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var encoding = deserializer.FromField<DateTimeNumericEncoding>(ffc.GetByOrdinal(1));
            var times = ffc.GetValue<T[]>(2);
            var values = ffc.GetValue<double[]>(3);

            return System.Tuple.Create(encoding, times, values);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}