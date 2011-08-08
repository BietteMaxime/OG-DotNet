//-----------------------------------------------------------------------
// <copyright file="BondSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Security.Bond
{
    /// <summary>
    /// TODO DOTNET-5: kill these with the .proto replacement
    /// </summary>
    public class BondSecurity : FinancialSecurity
    {
        private readonly string _issuerName;
        private readonly string _issuerType;
        private readonly string _issuerDomicile;
        private readonly string _market;
        private readonly string _currency;

        public BondSecurity(string name, string securityType, UniqueIdentifier uniqueId, ExternalIdBundle identifiers,
                            string issuerName, string issuerType, string issuerDomicile, string market, string currency // ... you get the idea, I'm not going to write all of these out
            ) : base(name, securityType, uniqueId, identifiers)
        {
            _issuerName = issuerName;
            _issuerType = issuerType;
            _issuerDomicile = issuerDomicile;
            _market = market;
            _currency = currency;
        }

        public string IssuerName
        {
            get { return _issuerName; }
        }

        public string IssuerType
        {
            get { return _issuerType; }
        }

        public string IssuerDomicile
        {
            get { return _issuerDomicile; }
        }

        public string Market
        {
            get { return _market; }
        }

        public string Currency
        {
            get { return _currency; }
        }
    }
}