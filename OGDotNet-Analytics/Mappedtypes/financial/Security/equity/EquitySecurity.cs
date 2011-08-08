//-----------------------------------------------------------------------
// <copyright file="EquitySecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Security.Equity
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class EquitySecurity : FinancialSecurity
    {
        public EquitySecurity(string name, string securityType, UniqueIdentifier uniqueId, ExternalIdBundle identifiers) : base(name, securityType, uniqueId, identifiers)
        {
        }
    }
}