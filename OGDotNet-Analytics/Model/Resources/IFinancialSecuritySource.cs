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