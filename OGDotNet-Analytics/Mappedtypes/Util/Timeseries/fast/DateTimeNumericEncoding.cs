//-----------------------------------------------------------------------
// <copyright file="DateTimeNumericEncoding.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Util.Timeseries.Fast
{
    public class DateTimeNumericEncoding
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private readonly Encoding _encoding;

        private enum Encoding
        {//TODO other encodings
            DateEpochDays
        }

        private DateTimeNumericEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }

        public static DateTimeNumericEncoding FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string value = ffc.GetString(1);

            return Build(value);
        }

        private static DateTimeNumericEncoding Build(string name)
        {
            return new DateTimeNumericEncoding(EnumBuilder<Encoding>.Parse(name));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
        
        public DateTime ConvertToDateTime(long sourceValue)
        {
            var daysSinceEpoch = ConvertToLong(sourceValue, Encoding.DateEpochDays); // TODO don't use days
            return Epoch + TimeSpan.FromDays(daysSinceEpoch);
        }

        public long ConvertToLong(long sourceValue, DateTimeNumericEncoding targetEncoding)
        {
            return ConvertToLong(sourceValue, targetEncoding._encoding);
        }

        private long ConvertToLong(long sourceValue, Encoding targetEncoding)
        {
            if (targetEncoding == _encoding)
                return sourceValue;
            throw new NotImplementedException();
        }
    }
}