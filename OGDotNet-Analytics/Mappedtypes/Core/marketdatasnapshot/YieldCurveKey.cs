using System;
using Fudge;
using Fudge.Serialization;
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class YieldCurveKey : IComparable<YieldCurveKey>
    {
        private readonly Currency _currency;
        private readonly string _name;

        public YieldCurveKey(Currency currency, string name)
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


        public static YieldCurveKey FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new YieldCurveKey(Currency.Create(ffc.GetString("currency")), ffc.GetString("name"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("currency",_currency.ISOCode);
            a.Add("name",Name);
        }

        public int CompareTo(YieldCurveKey other)
        {
            int ret = _currency.ISOCode.CompareTo(other._currency.ISOCode);
            if (ret != 0)
                return ret;
            return _name.CompareTo(other._name);
        }

        public bool Equals(YieldCurveKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._currency, _currency) && Equals(other._name, _name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (YieldCurveKey)) return false;
            return Equals((YieldCurveKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_currency != null ? _currency.GetHashCode() : 0)*397) ^ (_name != null ? _name.GetHashCode() : 0);
            }
        }
    }
}
