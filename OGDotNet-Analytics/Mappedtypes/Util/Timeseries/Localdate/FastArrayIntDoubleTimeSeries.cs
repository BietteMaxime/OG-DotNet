//-----------------------------------------------------------------------
// <copyright file="FastArrayIntDoubleTimeSeries.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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

namespace OGDotNet.Mappedtypes.Util.Timeseries.fast.integer
{
    public class FastArrayIntDoubleTimeSeries : FastArrayTDoubleTimeSeries<int>
    {
        private FastArrayIntDoubleTimeSeries(DateTimeNumericEncoding encoding, int[] times, double[] values)
            : base(encoding, times, values)
        {
        }

        public override IList<Tuple<DateTime, double>> Values
        {
            get
            {
                return Times.Select(t => Encoding.ConvertToDateTime(t)).Zip(_values, (t, v) => new Tuple<DateTime, double>(t, v)).ToList();
            }
        }

        public static FastArrayIntDoubleTimeSeries FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var bits = FromFudgeMsgImpl(ffc, deserializer);
            return new FastArrayIntDoubleTimeSeries(bits.Item1, bits.Item2, bits.Item3);
        }
    }
}