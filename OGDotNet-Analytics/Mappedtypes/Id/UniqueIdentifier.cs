//-----------------------------------------------------------------------
// <copyright file="UniqueIdentifier.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    public class UniqueIdentifier : IComparable<UniqueIdentifier>, IEquatable<UniqueIdentifier>, IComparable, IUniqueIdentifiable
    {
        private const string Separator = "~";
        static readonly string[] SeparatorArray = new[] { Separator };

        private const string SchemeFudgeFieldName = "Scheme";
        private const string ValueFudgeFieldName = "Value";
        private const string VersionFudgeFieldName = "Version";

        private readonly string _scheme;
        private readonly string _value;
        private readonly string _version;

        public static UniqueIdentifier Of(string scheme, string value, string version = null)
        {
            return new UniqueIdentifier(scheme, value, version);
        }

        public static UniqueIdentifier Of(Identifier id)
        {
            return new UniqueIdentifier(id.Scheme, id.Value, null);
        }

        public static UniqueIdentifier Parse(string uidStr)
        {
            ArgumentChecker.NotEmpty(uidStr, "uidStr");
            string[] split = uidStr.Split(SeparatorArray, StringSplitOptions.None);
            switch (split.Length)
            {
                case 2:
                    return Of(split[0], split[1]);
                case 3:
                    return Of(split[0], split[1], split[2]);
            }
            throw new ArgumentException("Invalid identifier format: " + uidStr);
        }

        private UniqueIdentifier(string scheme, string value, string version)
        {
            ArgumentChecker.NotEmpty(scheme, "scheme");
            ArgumentChecker.NotEmpty(value, "value");
            _scheme = string.Intern(scheme); //Should be a small static set
            _value = value;
            _version = StringUtils.TrimToNull(version);
        }

        //-------------------------------------------------------------------------
        public string Scheme
        {
            get { return _scheme; }
        }

        public string Value
        {
            get { return _value; }
        }

        public string Version
        {
            get { return _version; }
        }

        public bool IsLatest
        {
            get { return _version == null; }
        }

        public bool IsVersioned
        {
            get
            {
                return _version != null;
            }
        }

        public UniqueIdentifier ToLatest()
        {
            if (IsVersioned)
            {
                return new UniqueIdentifier(_scheme, _value, null);
            }
            else
            {
                return this;
            }
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder()
                .Append(_scheme).Append(Separator).Append(_value);
            if (_version != null)
            {
                buf.Append(Separator).Append(_version);
            }
            return buf.ToString();
        }

        public UniqueIdentifier UniqueId
        {
            get { return this; }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            var uniqueIdentifier = obj as UniqueIdentifier;
            if (uniqueIdentifier == null)
            {
                throw new ArgumentException(string.Format("Unexpected type {0}", obj.GetType()), "obj");
            }

            return CompareTo(uniqueIdentifier);
        }

        #region auto generated equality

        public int CompareTo(UniqueIdentifier other)
        {
            //NOTE: the aim here is to make compare work the same was as in java, which is ~InvariantCulture

            const StringComparison comparison = StringComparison.InvariantCulture;

            var schemeCompare = string.Compare(_scheme, other._scheme, comparison);
            if (schemeCompare != 0)
            {
                return schemeCompare;
            }
            var valueCompare = string.Compare(_value, other._value, comparison);
            if (valueCompare != 0)
            {
                return valueCompare;
            }

            return string.Compare(_version, other._version, comparison); // This handles null the same as the java CompareUtils class
        }

        public bool Equals(UniqueIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._scheme, _scheme) && Equals(other._value, _value) && Equals(other._version, _version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(UniqueIdentifier)) return false;
            return Equals((UniqueIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _scheme != null ? _scheme.GetHashCode() : 0;
                result = (result * 397) ^ (_value != null ? _value.GetHashCode() : 0);
                result = (result * 397) ^ (_version != null ? _version.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(UniqueIdentifier left, UniqueIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UniqueIdentifier left, UniqueIdentifier right)
        {
            return !Equals(left, right);
        }
        #endregion

        public static UniqueIdentifier FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string schema = null; 
            string value = null;
            string version = null;

            foreach (var field in ffc.GetAllFields())
            {
                switch (field.Name)
                {
                    case SchemeFudgeFieldName:
                        schema = (string) field.Value;
                        break;
                    case ValueFudgeFieldName:
                        value = (string) field.Value;
                        break;
                    case VersionFudgeFieldName:
                        version = (string)field.Value;
                        break;
                    case null:
                        if (field.Ordinal != 0)
                            throw new ArgumentException();
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new UniqueIdentifier(schema, value, version);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add(SchemeFudgeFieldName, Scheme);
            a.Add(ValueFudgeFieldName, Value);
            if (Version != null)
            {
                a.Add(VersionFudgeFieldName, Version);
            }
        }
    }
}