﻿//-----------------------------------------------------------------------
// <copyright file="LocalDate.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.JavaX.Time.Calendar
{
    public class LocalDate : IEquatable<LocalDate>, IComparable<LocalDate>, IComparable
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
            s.WriteTypeHeader(a, typeof(LocalDate));
            a.Add("date", new FudgeDate(_date));
        }

        public bool Equals(LocalDate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._date.Equals(_date);
        }

        public int CompareTo(LocalDate other)
        {
            return _date.CompareTo(other._date);
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

        public int CompareTo(object obj)
        {
            return CompareTo((LocalDate) obj);
        }

        public override string ToString()
        {
            return _date.ToShortDateString();
        }
    }
}
