using System;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Id
{
    [FudgeSurrogate(typeof(IdentifierBuilder))]
    public class Identifier : IEquatable<Identifier>, IComparable<Identifier>
    {
        private readonly string _scheme;
        public string Scheme
        {
            get { return _scheme; }
        }

        private readonly string _value;
        public string Value
        {
            get { return _value; }
        }

        public Identifier(string scheme, string value)
        {
            _scheme = scheme;
            _value = value;
        }

        public static Identifier Of(string scheme, string value)
        {
            return new Identifier(scheme,value);
        }

        public static Identifier Parse(string s)
        {
            string[] strings = s.Split(new[]{"::"},StringSplitOptions.None);
            return new Identifier(strings[0],strings[1]);
        }

        public int CompareTo(Identifier other)
        {
            //NOTE: the aim here is to make compare work the same was as in java, which is ~InvariantCulture

            const StringComparison comparison = StringComparison.InvariantCulture;

            var schemeCompare = string.Compare(_scheme, other._scheme, comparison);
            if (schemeCompare != 0)
            {
                return schemeCompare;
            }
            return string.Compare(_value, other._value, comparison);
        }

        public override string ToString()
        {
            return string.Format("{0}::{1}", Scheme, Value);
        }

        public bool Equals(Identifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._scheme, _scheme) && Equals(other._value, _value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Identifier)) return false;
            return Equals((Identifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_scheme != null ? _scheme.GetHashCode() : 0)*397) ^ (_value != null ? _value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Identifier left, Identifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Identifier left, Identifier right)
        {
            return !Equals(left, right);
        }


    }
}