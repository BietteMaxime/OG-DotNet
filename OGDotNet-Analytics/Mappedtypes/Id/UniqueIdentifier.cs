using System;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Utils;

namespace OGDotNet_Analytics.Mappedtypes.Id
{
    public class UniqueIdentifier : IComparable<UniqueIdentifier>, IEquatable<UniqueIdentifier>
    {

        public const String SCHEME_FUDGE_FIELD_NAME = "Scheme";
        /**
         * Fudge message key for the value.
         */
        public const String VALUE_FUDGE_FIELD_NAME = "Value";
        /**
         * Fudge message key for the version.
         */
        public const String VERSION_FUDGE_FIELD_NAME = "Version";

        /**
         * The scheme that categorizes the identifier value.
         */
        private readonly String _scheme;
        /**
         * The identifier value within the scheme.
         */
        private readonly String _value;
        /**
         * The version of the identifier, null if latest or not-versioned.
         */
        private readonly String _version;

        /**
         * Obtains an identifier from a scheme and value indicating the latest version
         * of the identifier, also used for non-versioned identifiers.
         * 
         * @param scheme  the scheme of the identifier, not empty, not null
         * @param value  the value of the identifier, not empty, not null
         * @return the identifier, not null
         */
        public static UniqueIdentifier Of(String scheme, String value)
        {
            return Of(scheme, value, null);
        }

        /**
         * Obtains an identifier from a scheme, value and version.
         * 
         * @param scheme  the scheme of the identifier, not empty, not null
         * @param value  the value of the identifier, not empty, not null
         * @param version  the version of the identifier, empty treated as null, null treated as latest version
         * @return the identifier, not null
         */
        public static UniqueIdentifier Of(String scheme, String value, String version)
        {
            return new UniqueIdentifier(scheme, value, version);
        }

        /**
         * Obtains an identifier from a formatted scheme and value.
         * <p>
         * This parses the identifier from the form produced by {@code toString()}
         * which is {@code <SCHEME>::<VALUE>::<VERSION>}.
         * 
         * @param uidStr  the identifier to parse, not null
         * @return the identifier, not null
         * @throws IllegalArgumentException if the identifier cannot be parsed
         */
        public static UniqueIdentifier Parse(String uidStr)
        {

            ArgumentChecker.NotEmpty(uidStr, "uidStr");
            String[] split = uidStr.Split(new string[] { "::" }, StringSplitOptions.None);
            switch (split.Length)
            {
                case 2:
                    return Of(split[0], split[1], null);
                case 3:
                    return Of(split[0], split[1], split[2]);
            }
            throw new ArgumentException("Invalid identifier format: " + uidStr);
        }

        /**
         * Creates an instance.
         * 
         * @param scheme  the scheme of the identifier, not empty, not null
         * @param value  the value of the identifier, not empty, not null
         * @param version  the version of the identifier, null if latest version
         */
        public UniqueIdentifier(String scheme, String value, String version)
        {
            ArgumentChecker.NotEmpty(scheme, "scheme");
            ArgumentChecker.NotEmpty(value, "value");
            _scheme = scheme;
            _value = value;
            _version = StringUtils.TrimToNull(version);
        }

        //-------------------------------------------------------------------------
        /**
         * Gets the scheme of the identifier.
         * <p>
         * This is extracted from the object identifier.
         * This is not expected to be the same as {@code IdentificationScheme}.
         * 
         * @return the scheme, not empty, not null
         */
        public String Scheme
        {
            get { return _scheme; }
        }

        /**
         * Gets the value of the identifier.
         * <p>
         * This is extracted from the object identifier.
         * 
         * @return the value, not empty, not null
         */
        public String Value
        {
            get { return _value; }
        }

        /**
         * Gets the version of the identifier.
         * 
         * @return the version, null if latest version
         */
        public String Version
        {
            get { return _version; }
        }

        /**
         * Checks if this represents the latest version of the item.
         * <p>
         * This simply checks if the version is null.
         * 
         * @return true if this is the latest version
         */
        public bool IsLatest
        {
            get { return _version == null; }
        }

        /**
         * Checks if this represents a versioned reference to the item.
         * <p>
         * This simply checks if the version is non null.
         * 
         * @return true if this is a versioned reference
         */
        public bool IsVersioned
        {
            get
            {
                return _version != null;
            }
        }

        /**
         * Returns a unique identifier based on this with the version set to null.
         * <p>
         * The returned identifier will represent the latest version of the item.
         * 
         * @return an identifier representing the latest version of the item, not null
         */
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

        public override String ToString()
        {
            StringBuilder buf = new StringBuilder()
                .Append(_scheme).Append(':').Append(':').Append(_value);
            if (_version != null)
            {
                buf.Append(':').Append(':').Append(_version);
            }
            return buf.ToString();
        }

        public UniqueIdentifier UniqueId
        {
            get { return this; }
        }

        #region auto generated equality

        public int CompareTo(UniqueIdentifier other)
        {//TODO Aggh, cuulture and CompareTo in java land vs .Net
            if (_scheme.CompareTo(other._scheme) != 0)
            {
                return _scheme.CompareTo(other._scheme);
            }
            if (_value.CompareTo(other._value) != 0)
            {
                return _value.CompareTo(other._value);
            }
            return CompareUtils.CompareWithNull(_version, other._version);
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
                int result = (_scheme != null ? _scheme.GetHashCode() : 0);
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
                    case SCHEME_FUDGE_FIELD_NAME:
                        schema = (string) field.Value;
                        break;
                    case VALUE_FUDGE_FIELD_NAME:
                        value = (string) field.Value;
                        break;
                    case VERSION_FUDGE_FIELD_NAME:
                        version = (string)field.Value;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new UniqueIdentifier(schema, value, version);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add(SCHEME_FUDGE_FIELD_NAME, Scheme);
            a.Add(VALUE_FUDGE_FIELD_NAME, Value);
            if (Version != null)
            {
                a.Add(VERSION_FUDGE_FIELD_NAME, Version);
            }
        }
    }
}