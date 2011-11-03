//-----------------------------------------------------------------------
// <copyright file="ObjectId.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Id
{
    public class ObjectId : IEquatable<ObjectId>
    {
        private const string Separator = "~";
        static readonly string[] SeparatorArray = new[] { Separator };

        private const string SchemeFudgeFieldName = "Scheme";
        private const string ValueFudgeFieldName = "Value";

        //TODO the rest of this

        private readonly string _scheme;
        private readonly string _value;

        private ObjectId(string scheme, string value)
        {
            _scheme = scheme;
            _value = value;
        }

        public static ObjectId Create(string scheme, string value)
        {
            ArgumentChecker.NotNull(scheme, "scheme");
            ArgumentChecker.NotNull(value, "value");

            return new ObjectId(scheme, value);
        }

        public static ObjectId Parse(string uidStr)
        {
            ArgumentChecker.NotEmpty(uidStr, "uidStr");
            string[] split = uidStr.Split(SeparatorArray, StringSplitOptions.None);
            switch (split.Length)
            {
                case 2:
                    return Create(split[0], split[1]);
            }
            throw new ArgumentException("Invalid identifier format: " + uidStr);
        }

        public string Scheme
        {
            get { return _scheme; }
        }

        public string Value
        {
            get { return _value; }
        }

        public bool Equals(ObjectId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _scheme.Equals(other._scheme) && _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ObjectId)) return false;
            return Equals((ObjectId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_scheme.GetHashCode() * 397) ^ _value.GetHashCode();
            }
        }

        public static ObjectId FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string schema = null;
            string value = null;

            foreach (var field in ffc)
            {
                switch (field.Name)
                {
                    case SchemeFudgeFieldName:
                        schema = (string)field.Value;
                        break;
                    case ValueFudgeFieldName:
                        value = (string)field.Value;
                        break;
                    case null:
                        if (field.Ordinal != 0)
                            throw new ArgumentException();
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new ObjectId(schema, value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add(SchemeFudgeFieldName, Scheme);
            a.Add(ValueFudgeFieldName, Value);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder()
                .Append(_scheme).Append(Separator).Append(_value);
            return buf.ToString();
        }
    }
}
