//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceKey.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.cube
{
    public class VolatilitySurfaceKey : IComparable<VolatilitySurfaceKey>
    {
        private readonly Currency _currency;
        private readonly string _name;
        private readonly string _instrumentType;

        public VolatilitySurfaceKey(Currency currency, string name, string instrumentType)
        {
            _currency = currency;
            _name = name;
            _instrumentType = instrumentType;
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string InstrumentType
        {
            get { return _instrumentType; }
        }

        public override string ToString()
        {
            return string.Format("[VolatilitySurfaceKey {0} {1} {2}]", _currency.ISOCode, _name, _instrumentType);
        }
        public static VolatilitySurfaceKey FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilitySurfaceKey(ffc.GetValue<Currency>("currency"), ffc.GetString("name"), ffc.GetString("instrumentType"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("currency", Currency);
            a.Add("name", Name);
            a.Add("instrumentType", InstrumentType);
        }

        public int CompareTo(VolatilitySurfaceKey other)
        {
            int ret = _currency.ISOCode.CompareTo(other._currency.ISOCode);
            if (ret != 0)
                return ret;
            return _name.CompareTo(other._name);
        }

        public bool Equals(VolatilitySurfaceKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._currency, _currency) && Equals(other._name, _name) && Equals(other._instrumentType, _instrumentType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolatilitySurfaceKey)) return false;
            return Equals((VolatilitySurfaceKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_currency != null ? _currency.GetHashCode() : 0) * 397) ^ (_name != null ? _name.GetHashCode() : 0);
            }
        }
    }
}