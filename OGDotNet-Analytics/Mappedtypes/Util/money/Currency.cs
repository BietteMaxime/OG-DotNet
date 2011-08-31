//-----------------------------------------------------------------------
// <copyright file="Currency.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Util.Money
{
    public class Currency : IEquatable<Currency>, IUniqueIdentifiable
    {
        private const string IdentificationDomain = "CurrencyISO";
        private static readonly Memoizer<string, Currency> InstanceMap = new Memoizer<string, Currency>(GetInstanceImpl);

        // a selection of commonly traded, stable currencies

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The currency 'USD' - United States Dollar.
        /// </summary>
        public static readonly Currency USD = Create("USD");

        /// <summary>
        /// The currency 'EUR' - Euro.
        /// </summary>
        public static readonly Currency EUR = Create("EUR");

        /// <summary>
        /// The currency 'JPY' - Japanese Yen.
        /// </summary>
        public static readonly Currency JPY = Create("JPY");

        /// <summary>
        /// The currency 'GBP' - British pound.
        /// </summary>
        public static readonly Currency GBP = Create("GBP");

        /// <summary>
        /// The currency 'EUR' - Swiss Franc.
        /// </summary>
        public static readonly Currency CHF = Create("CHF");

        /// <summary>
        /// The currency 'AUD' - Australian Dollar.
        /// </summary>
        public static readonly Currency AUD = Create("AUD");

        /// <summary>
        /// The currency 'CAD' - Canadian Dollar.
        /// </summary>
        public static readonly Currency CAD = Create("CAD");

        /// <summary>
        /// The currency 'DKK' - Dutch Krone
        /// </summary>
        public static readonly Currency DKK = Create("DKK");

        /// <summary>
        /// The currency 'DEM' - Deutsche Mark
        /// </summary>
        public static readonly Currency DEM = Create("DEM");

        /// <summary>
        /// The currency 'CZK' - Czeck Krona
        /// </summary>
        public static readonly Currency CZK = Create("CZK");

        /// <summary>
        /// The currency 'SEK' - Swedish Krona
        /// </summary>
        public static readonly Currency SEK = Create("SEK");

        /// <summary>
        /// The currency 'SKK' - Slovak Korona
        /// </summary>
        public static readonly Currency SKK = Create("SKK");

        /// <summary>
        /// The currency 'ITL' - Italian Lira
        /// </summary>
        public static readonly Currency ITL = Create("ITL");

        /// <summary>
        /// The currency 'HUF' = Hugarian Forint
        /// </summary>
        public static readonly Currency HUF = Create("HUF");

        /// <summary>
        /// The currency 'HKD' - Hong Kong Dollar
        /// </summary>
        public static readonly Currency HKD = Create("HKD");

        /// <summary>
        /// The currency 'FRF' - French Franc
        /// </summary>
        public static readonly Currency FRF = Create("FRF");

        /// <summary>
        /// The currency 'NOK' - Norwegian Krone 
        /// </summary>
        public static readonly Currency NOK = Create("NOK");

        // ReSharper restore InconsistentNaming

        private static Currency GetInstanceImpl(string isoCode)
        {
            ArgumentChecker.NotEmpty(isoCode, "isoCode");
            if (isoCode.Length != 3)
            {
                throw new ArgumentOutOfRangeException("Invalid ISO code: " + isoCode);
            }

            return new Currency(isoCode.ToUpper(CultureInfo.CreateSpecificCulture("en")));
        }

        public static Currency Create(UniqueId isoCode)
        {
            if (isoCode.Scheme != IdentificationDomain)
                throw new ArgumentException("Unexpected Scheme", "isoCode");
            if (isoCode.IsVersioned)
                throw new ArgumentException("Unexpected Versioned UID", "isoCode");

            Currency ret = Create(isoCode.Value);

            if (!ret.UniqueId.Equals(isoCode))
                throw new ArgumentException("Unexpected UID", "isoCode");

            return ret;
        }

        public static Currency Create(string isoCode)
        {
            return InstanceMap.Get(isoCode);
        }

        private readonly UniqueId _uniqueId;

        private Currency(string isoCode)
        {
            _uniqueId = UniqueId.Create(IdentificationDomain, isoCode);
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }
        public string ISOCode
        {
            get { return _uniqueId.Value; }
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
            return _uniqueId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Currency: {0}]", _uniqueId.Value);
        }
    }
}