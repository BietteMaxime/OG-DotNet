// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EquitySecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Financial.Security.Equity
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class EquitySecurity : FinancialSecurity
    {
        private readonly string _shortName;
        public EquitySecurity(string name, string securityType, UniqueId uniqueId, ExternalIdBundle identifiers, string shortName) : base(name, securityType, uniqueId, identifiers)
        {
            _shortName = shortName;
        }

        public string ShortName
        {
            get { return _shortName; }
        }
    }
}