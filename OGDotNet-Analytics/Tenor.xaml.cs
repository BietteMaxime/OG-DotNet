using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics
{
    public class Tenor : IEquatable<Tenor>
    {
        private readonly string _period;

        private Tenor(string period)
        {
            _period = period;
        }

        public static Tenor FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new Tenor(ffc.GetValue<string>("tenor"));//TODO Period type
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            return _period;
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
            if (obj.GetType() != typeof (Tenor)) return false;
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