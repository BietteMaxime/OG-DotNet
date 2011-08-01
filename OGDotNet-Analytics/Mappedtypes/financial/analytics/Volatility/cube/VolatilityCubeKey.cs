//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeKey.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.cube
{
    public class VolatilityCubeKey : IComparable<VolatilityCubeKey>
    {
        private readonly Currency _currency;
        private readonly string _name;

        public VolatilityCubeKey(Currency currency, string name)
        {
            _currency = currency;
            _name = name;
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public string Name
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return string.Format("[VolatilityCubeKey {0} {1}]", _currency.ISOCode, _name);
        }
        public static VolatilityCubeKey FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilityCubeKey(ffc.GetValue<Currency>("currency"), ffc.GetString("name"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("currency", _currency);
            a.Add("name", Name);
        }

        public int CompareTo(VolatilityCubeKey other)
        {
            int ret = _currency.ISOCode.CompareTo(other._currency.ISOCode);
            if (ret != 0)
                return ret;
            return _name.CompareTo(other._name);
        }

        public bool Equals(VolatilityCubeKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._currency, _currency) && Equals(other._name, _name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolatilityCubeKey)) return false;
            return Equals((VolatilityCubeKey)obj);
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
