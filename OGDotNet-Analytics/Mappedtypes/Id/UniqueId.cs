//-----------------------------------------------------------------------
// <copyright file="UniqueId.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    public class UniqueId : IComparable<UniqueId>, IEquatable<UniqueId>, IComparable, IUniqueIdentifiable
    {
        private const string Separator = "~";
        static readonly string[] SeparatorArray = new[] { Separator };

        private const string SchemeFudgeFieldName = "Scheme";
        private const string ValueFudgeFieldName = "Value";
        private const string VersionFudgeFieldName = "Version";

        private readonly string _scheme;
        private readonly string _value;
        private readonly string _version;

        public static UniqueId Create(string scheme, string value, string version = null)
        {
            return new UniqueId(scheme, value, version);
        }

        public static UniqueId Create(ObjectId objectId, string version = null)
        {
            return Create(objectId.Scheme, objectId.Value, version);
        }

        public static UniqueId Parse(string uidStr)
        {
            ArgumentChecker.NotEmpty(uidStr, "uidStr");
            string[] split = uidStr.Split(SeparatorArray, StringSplitOptions.None);
            switch (split.Length)
            {
                case 2:
                    return Create(split[0], split[1]);
                case 3:
                    return Create(split[0], split[1], split[2]);
            }
            throw new ArgumentException("Invalid identifier format: " + uidStr);
        }

        private UniqueId(string scheme, string value, string version)
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

        public UniqueId ToLatest()
        {
            if (IsVersioned)
            {
                return new UniqueId(_scheme, _value, null);
            }
            else
            {
                return this;
            }
        }

        public ExternalId ToIdentifier()
        {
            return ExternalId.Create("UID", ToString());
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

        UniqueId IUniqueIdentifiable.UniqueId
        {
            get { return this; }
        }

        public int CompareTo(object obj)
        {
            ArgumentChecker.NotNull(obj, "obj");
            var uniqueIdentifier = obj as UniqueId;
            if (uniqueIdentifier == null)
            {
                throw new ArgumentException(string.Format("Unexpected type {0}", obj.GetType()), "obj");
            }

            return CompareTo(uniqueIdentifier);
        }

        public ObjectId ObjectID
        {
            get
            {
                return ObjectId.Create(_scheme, _value);
            }
        }

        #region auto generated equality

        public int CompareTo(UniqueId other)
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

        public bool Equals(UniqueId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._scheme.Equals(_scheme) && other._value.Equals(_value) && Equals(other._version, _version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(UniqueId)) return false;
            return Equals((UniqueId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _scheme.GetHashCode();
                result = (result * 397) ^ _value.GetHashCode();
                result = (result * 397) ^ (_version != null ? _version.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(UniqueId left, UniqueId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UniqueId left, UniqueId right)
        {
            return !Equals(left, right);
        }
        #endregion

        public static UniqueId FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string schema = null;
            string value = null;
            string version = null;

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
            return new UniqueId(schema, value, version);
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