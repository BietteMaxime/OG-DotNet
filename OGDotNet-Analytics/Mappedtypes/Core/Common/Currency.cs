using System;
using System.Globalization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.Common
{
    public class Currency : IEquatable<Currency>
    {
        private const String IdentificationDomain = "CurrencyISO";
        private readonly static Memoizer<String, Currency> InstanceMap = new Memoizer<String, Currency>(GetInstanceImpl);

        private static Currency GetInstanceImpl(string isoCode)
        {
            ArgumentChecker.NotEmpty(isoCode, "ISO Code");
            if (isoCode.Length != 3)
            {
                throw new ArgumentOutOfRangeException("Invalid ISO code: " + isoCode);
            }

            return new Currency(isoCode.ToUpper(CultureInfo.CreateSpecificCulture("en")));
        }

        public static Currency GetInstance(string isoCode)
        {
            return InstanceMap.Get(isoCode);
        }

        private readonly UniqueIdentifier _identifier;

        private Currency(string isoCode)
        {
            _identifier = UniqueIdentifier.Of(IdentificationDomain, isoCode);
        }

        public UniqueIdentifier Identifier
        {
            get { return _identifier; }
        }
        public string ISOCode
        {
            get { return _identifier.Value; }
        }

        public bool Equals(Currency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //Singletoning means that we don't need to check other things
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Currency)) return false;
            return Equals((Currency) obj);
        }

        public override int GetHashCode()
        {
            return _identifier.GetHashCode();
        }
    }
}
