//-----------------------------------------------------------------------
// <copyright file="LocalDate.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.javax.time.calendar
{
    public class LocalDate : IEquatable<LocalDate>
    {
        private readonly DateTime _date;

        private LocalDate(DateTime date)
        {
            _date = date;
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public static LocalDate FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new LocalDate(DateTime.Parse(ffc.GetString("date")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public bool Equals(LocalDate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._date.Equals(_date);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LocalDate)) return false;
            return Equals((LocalDate) obj);
        }

        public override int GetHashCode()
        {
            return _date.GetHashCode();
        }
    }
}
