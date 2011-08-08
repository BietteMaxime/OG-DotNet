//-----------------------------------------------------------------------
// <copyright file="ExternalId.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Id
{
    [FudgeSurrogate(typeof(ExternalIdBuilder))]
    public class ExternalId : IEquatable<ExternalId>, IComparable<ExternalId>
    {
        private const string Separator = "~";

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

        public ExternalId(string scheme, string value)
        {
            _scheme = scheme;
            _value = value;
        }

        public static ExternalId Of(string scheme, string value)
        {
            return new ExternalId(scheme, value);
        }

        public static ExternalId Parse(string s)
        {
            int pos = s.IndexOf(Separator);
            if (pos < 0)
            {
                throw new ArgumentException(string.Format("Invalid identifier format: {0}", s), "s");
            }
            return new ExternalId(s.Substring(0, pos), s.Substring(pos + 1));
        }

        public int CompareTo(ExternalId other)
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
            return string.Format("{0}{1}{2}", Scheme, Separator, Value);
        }

        public bool Equals(ExternalId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._scheme, _scheme) && Equals(other._value, _value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ExternalId)) return false;
            return Equals((ExternalId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_scheme != null ? _scheme.GetHashCode() : 0) * 397) ^ (_value != null ? _value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ExternalId left, ExternalId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExternalId left, ExternalId right)
        {
            return !Equals(left, right);
        }
    }
}