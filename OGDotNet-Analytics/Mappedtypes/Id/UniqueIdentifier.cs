using System;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet_Analytics.Utils;

namespace OGDotNet_Analytics.Mappedtypes.Id
{
    /// <summary>
    /// TODO: .netify this and finish it
    /// </summary>
    public class UniqueIdentifier : IComparable<UniqueIdentifier>, IEquatable<UniqueIdentifier>
    {

        private const String SchemeFudgeFieldName = "Scheme";
        private const String ValueFudgeFieldName = "Value";
        private const String VersionFudgeFieldName = "Version";

        private readonly String _scheme;
        private readonly String _value;
        private readonly String _version;

        public static UniqueIdentifier Of(String scheme, String value)
        {
            return Of(scheme, value, null);
        }

        public static UniqueIdentifier Of(String scheme, String value, String version)
        {
            return new UniqueIdentifier(scheme, value, version);
        }

        public static UniqueIdentifier Parse(String uidStr)
        {

            ArgumentChecker.NotEmpty(uidStr, "uidStr");
            String[] split = uidStr.Split(new string[] { "::" }, StringSplitOptions.None);
            switch (split.Length)
            {
                case 2:
                    return Of(split[0], split[1]);
                case 3:
                    return Of(split[0], split[1], split[2]);
            }
            throw new ArgumentException("Invalid identifier format: " + uidStr);
        }

        private UniqueIdentifier(String scheme, String value, String version)
        {
            ArgumentChecker.NotEmpty(scheme, "scheme");
            ArgumentChecker.NotEmpty(value, "value");
            _scheme = scheme;
            _value = value;
            _version = StringUtils.TrimToNull(version);
        }

        //-------------------------------------------------------------------------
        public String Scheme
        {
            get { return _scheme; }
        }

        public String Value
        {
            get { return _value; }
        }

        public String Version
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

            return string.Compare(_version, other._version, comparison);//This handles null the same as the java CompareUtils class
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
                    case SchemeFudgeFieldName:
                        schema = (string) field.Value;
                        break;
                    case ValueFudgeFieldName:
                        value = (string) field.Value;
                        break;
                    case VersionFudgeFieldName:
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
            a.Add(SchemeFudgeFieldName, Scheme);
            a.Add(ValueFudgeFieldName, Value);
            if (Version != null)
            {
                a.Add(VersionFudgeFieldName, Version);
            }
        }
    }
}