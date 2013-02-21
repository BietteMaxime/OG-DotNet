// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VolatilitySurfaceKey.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;

namespace OpenGamma.Financial.Analytics.Volatility.Cube
{
    public class VolatilitySurfaceKey : IComparable<VolatilitySurfaceKey>
    {
        private readonly UniqueId _target;
        private readonly string _name;
        private readonly string _instrumentType;
        private readonly string _quoteType;
        private readonly string _quoteUnits;

        public VolatilitySurfaceKey(UniqueId target, string name, string instrumentType, string quoteType, string quoteUnits)
        {
            _target = target;
            _name = name;
            _instrumentType = instrumentType;
            _quoteType = quoteType;
            _quoteUnits = quoteUnits;
        }

        public UniqueId Target
        {
            get { return _target; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string InstrumentType
        {
            get { return _instrumentType; }
        }

        public string QuoteType
        {
            get { return _quoteType; }
        }

        public string QuoteUnits
        {
            get { return _quoteUnits; }
        }

        public override string ToString()
        {
            return string.Format("[VolatilitySurfaceKey {0} {1} {2}]", _target.Value, _name, _instrumentType);
        }

        public static VolatilitySurfaceKey FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilitySurfaceKey(UniqueId.Parse(ffc.GetString("target")), ffc.GetString("name"), ffc.GetString("instrumentType"), ffc.GetString("quoteType"), ffc.GetString("quoteUnits"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("target", Target.ToString());
            a.Add("name", Name);
            a.Add("instrumentType", InstrumentType);
            a.Add("quoteType", QuoteType);
            a.Add("quoteUnits", QuoteUnits);
        }

        public int CompareTo(VolatilitySurfaceKey other)
        {
            int ret = _target.CompareTo(other._target);
            if (ret != 0)
                return ret;
            ret = string.CompareOrdinal(_name, other._name);
            if (ret != 0)
                return ret;
            return string.CompareOrdinal(_instrumentType, other._instrumentType);
        }

        public bool Equals(VolatilitySurfaceKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._target, _target) && Equals(other._name, _name) && Equals(other._instrumentType, _instrumentType) && Equals(other._quoteType, _quoteType) && Equals(other._quoteUnits, _quoteUnits);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolatilitySurfaceKey)) return false;
            return Equals((VolatilitySurfaceKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _target != null ? _target.GetHashCode() : 0;
                result = (result * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                result = (result * 397) ^ (_instrumentType != null ? _instrumentType.GetHashCode() : 0);
                result = (result * 397) ^ (_quoteType != null ? _quoteType.GetHashCode() : 0);
                result = (result * 397) ^ (_quoteUnits != null ? _quoteUnits.GetHashCode() : 0);
                return result;
            }
        }
    }
}