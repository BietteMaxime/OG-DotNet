//-----------------------------------------------------------------------
// <copyright file="ObjectID.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Id
{
    public class ObjectID : IEquatable<ObjectID>
    {
        //TODO the rest of this

        private readonly string _scheme;
        private readonly string _value;

        private ObjectID(string scheme, string value)
        {
            _scheme = scheme;
            _value = value;
        }

        public static ObjectID Create(string scheme, string value)
        {
            ArgumentChecker.NotNull(scheme, "scheme");
            ArgumentChecker.NotNull(value, "value");

            return new ObjectID(scheme, value);
        }

        public string Scheme
        {
            get { return _scheme; }
        }

        public string Value
        {
            get { return _value; }
        }

        public bool Equals(ObjectID other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _scheme.Equals(other._scheme) && _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ObjectID)) return false;
            return Equals((ObjectID) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_scheme.GetHashCode() * 397) ^ _value.GetHashCode();
            }
        }
    }
}
