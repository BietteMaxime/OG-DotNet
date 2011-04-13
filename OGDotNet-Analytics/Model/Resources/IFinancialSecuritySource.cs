//-----------------------------------------------------------------------
// <copyright file="IFinancialSecuritySource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Security;

namespace OGDotNet.Model.Resources
{
    public interface  IFinancialSecuritySource : ISecuritySource
    {
        IEnumerable<Security> GetBondsWithIssuerName(String issuerName);
    }
}