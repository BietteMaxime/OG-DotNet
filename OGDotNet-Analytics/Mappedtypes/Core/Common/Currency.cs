//-----------------------------------------------------------------------
// <copyright file="Currency.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.Common
{
    public class Currency : IEquatable<Currency>
    {
        private const string IdentificationDomain = "CurrencyISO";
        private static readonly Memoizer<string, Currency> InstanceMap = new Memoizer<string, Currency>(GetInstanceImpl);

        private static Currency GetInstanceImpl(string isoCode)
        {
            ArgumentChecker.NotEmpty(isoCode, "ISO Code");
            if (isoCode.Length != 3)
            {
                throw new ArgumentOutOfRangeException("Invalid ISO code: " + isoCode);
            }

            return new Currency(isoCode.ToUpper(CultureInfo.CreateSpecificCulture("en")));
        }


        public static Currency Create(UniqueIdentifier isoCode)
        {
            if (isoCode.Scheme != IdentificationDomain)
                throw new ArgumentException("Unexpected Scheme", "isoCode");
            if (isoCode.IsVersioned)
                throw new ArgumentException("Unexpected Versioned UID", "isoCode");

            Currency ret = Create(isoCode.Value);

            if (!ret.Identifier.Equals(isoCode))
                throw new ArgumentException("Unexpected UID", "isoCode");

            return ret;
        }

        public static Currency Create(string isoCode)
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
            if (obj.GetType() != typeof(Currency)) return false;
            return Equals((Currency)obj);
        }

        public override int GetHashCode()
        {
            return _identifier.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Currency: {0}]", _identifier.Value);
        }
    }
}
