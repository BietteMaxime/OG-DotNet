using System.Collections.Generic;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Security
{
    public interface ISecuritySource
    {
        Security GetSecurity(UniqueIdentifier uid);
        ICollection<Security> GetSecurities(IdentifierBundle bundle);
        Security GetSecurity(IdentifierBundle bundle);
    }
}