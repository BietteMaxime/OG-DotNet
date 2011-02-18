﻿using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Util.Timeseries.Localdate
{
    public class LocalDateEpochDaysConverter
    {
        private readonly TimeZoneInfo _timeZone;

        private LocalDateEpochDaysConverter(string timeZone)
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }

        public static LocalDateEpochDaysConverter FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new LocalDateEpochDaysConverter(ffc.GetString(1));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset Convert(DateTime localtime)
        {
            var utcOffset = _timeZone.GetUtcOffset(localtime);
            return new DateTimeOffset(localtime, utcOffset);
        }
    }
}