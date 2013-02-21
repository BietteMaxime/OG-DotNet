// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tenor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Util.Time
{
    public class Tenor : IEquatable<Tenor>, IComparable<Tenor>, IComparable
    {
        public static readonly Tenor Day = new Tenor("P1D");
        public static readonly Tenor OneDay = new Tenor("P1D");
        public static readonly Tenor TwoDays = new Tenor("P2D");
        public static readonly Tenor ThreeDays = new Tenor("P3D");
        public static readonly Tenor OneWeek = new Tenor("P7D");
        public static readonly Tenor TwoWeeks = new Tenor("P14D");
        public static readonly Tenor ThreeWeeks = new Tenor("P21D");
        public static readonly Tenor SixWeeks = new Tenor("P42D");
        public static readonly Tenor OneMonth = new Tenor("P1M");
        public static readonly Tenor TwoMonths = new Tenor("P2M");
        public static readonly Tenor ThreeMonths = new Tenor("P3M");
        public static readonly Tenor FourMonths = new Tenor("P4M");
        public static readonly Tenor FiveMonths = new Tenor("P5M");
        public static readonly Tenor SixMonths = new Tenor("P6M");
        public static readonly Tenor SevenMonths = new Tenor("P7M");
        public static readonly Tenor EightMonths = new Tenor("P8M");
        public static readonly Tenor NineMonths = new Tenor("P9M");
        public static readonly Tenor TenMonths = new Tenor("P10M");
        public static readonly Tenor ElevenMonths = new Tenor("P11M");
        public static readonly Tenor TwelveMonths = new Tenor("P12M");
        public static readonly Tenor EighteenMonths = new Tenor("P18M");
        public static readonly Tenor Year = new Tenor("P1Y");
        public static readonly Tenor OneYear = new Tenor("P1Y");
        public static readonly Tenor TwoYears = new Tenor("P2Y");
        public static readonly Tenor ThreeYears = new Tenor("P3Y");
        public static readonly Tenor FourYears = new Tenor("P4Y");
        public static readonly Tenor FiveYears = new Tenor("P5Y");
        public static readonly Tenor TenYears = new Tenor("P10Y");
        public static readonly Tenor WorkingWeek = new Tenor("P5D");

        private readonly string _period;
        
        public Tenor(string period) 
        {
            _period = period;
        }

        public static Tenor Create(string period)
        {
            return new Tenor(period);
        }

        public static Tenor FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new Tenor(ffc.GetValue<string>("tenor")); // TODO Period type
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(Tenor) );
            a.Add("tenor", _period);
        }

        public int CompareTo(Tenor other)
        {
            return TimeSpan.CompareTo(other.TimeSpan);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Tenor)obj);
        }

        public override string ToString()
        {
            return _period;
        }

        /// <summary>
        /// Gets  a TimeSpan that might vaguely represent the period of this tenor
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