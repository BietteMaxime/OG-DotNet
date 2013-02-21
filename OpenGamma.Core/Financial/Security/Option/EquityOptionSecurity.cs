// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EquityOptionSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Financial.Security.Option
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class EquityOptionSecurity : FinancialSecurity
    {
        private readonly ExternalId _underlyingIdentifier;

        public EquityOptionSecurity(string name, string securityType, UniqueId uniqueId, ExternalIdBundle identifiers, ExternalId underlyingIdentifier) : base(name, securityType, uniqueId, identifiers)
        {
            _underlyingIdentifier = underlyingIdentifier;
        }

        public ExternalId UnderlyingIdentifier
        {
            get { return _underlyingIdentifier; }
        }
    }
}
