//-----------------------------------------------------------------------
// <copyright file="Tenor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Util.Time
{
    public class Tenor : IEquatable<Tenor>, IComparable<Tenor>
    {
        public static readonly Tenor Day = new Tenor("P1D");
        public static readonly Tenor TwoYears = new Tenor("P2Y");

        private readonly string _period;
        
        public Tenor(string period) 
        {
            _period = period;
        }

        public static Tenor FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new Tenor(ffc.GetValue<string>("tenor")); // TODO Period type
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("tenor", _period);
        }

        public int CompareTo(Tenor other)
        {
            return TimeSpan.CompareTo(other.TimeSpan);
        }

        public override string ToString()
        {
            return _period;
        }

        /// <summary>
        /// Gets  a TimeSpan that might vaguelly represent the period of this tenor
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return System.Xml.XmlConvert.ToTimeSpan(_period); // This understands the ISO8601 standard, and does something vaguelly sane
            }
        }

        public bool Equals(Tenor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._period, _period);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Tenor)) return false;
            return Equals((Tenor) obj);
        }

        public override int GetHashCode()
        {
            return _period.GetHashCode();
        }

        public static bool operator ==(Tenor left, Tenor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tenor left, Tenor right)
        {
            return !Equals(left, right);
        }
    }
}