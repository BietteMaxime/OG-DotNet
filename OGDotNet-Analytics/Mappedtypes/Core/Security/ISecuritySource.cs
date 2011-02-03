using System.Collections.Generic;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Mappedtypes.Core.Security
{
    public interface ISecuritySource
    {
        Security GetSecurity(UniqueIdentifier uid);
        ICollection<Security> GetSecurities(IdentifierBundle bundle);
        Security GetSecurity(IdentifierBundle bundle);
    }
}