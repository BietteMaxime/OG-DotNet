//-----------------------------------------------------------------------
// <copyright file="FinancialSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.Security;

namespace OGDotNet.Mappedtypes.Financial.Security
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class FinancialSecurity : ManageableSecurity
    {
        public FinancialSecurity(string name, string securityType, UniqueId uniqueId, ExternalIdBundle identifiers) : base(name, securityType, uniqueId, identifiers)
        {
        }
    }
}