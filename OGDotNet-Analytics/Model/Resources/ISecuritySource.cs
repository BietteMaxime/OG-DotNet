using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public interface ISecuritySource
    {
        Security GetSecurity(UniqueIdentifier uid);
        ICollection<Security> GetSecurities(IdentifierBundle bundle);
        Security GetSecurity(IdentifierBundle bundle);
    }
}