//-----------------------------------------------------------------------
// <copyright file="EquityOptionSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Security.Option
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    class EquityOptionSecurity : FinancialSecurity
    {
        public EquityOptionSecurity(string name, string securityType, UniqueIdentifier uniqueId, IdentifierBundle identifiers) : base(name, securityType, uniqueId, identifiers)
        {
        }
    }
}
