// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;
using OpenGamma.Master.Security;

namespace OpenGamma.Financial.Security
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