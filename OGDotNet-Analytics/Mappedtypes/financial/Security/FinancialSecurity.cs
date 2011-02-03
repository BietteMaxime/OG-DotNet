using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Mappedtypes.Master.Security;

namespace OGDotNet_Analytics.Mappedtypes.financial.Security
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class FinancialSecurity : ManageableSecurity
    {
        public FinancialSecurity(string name, string securityType, UniqueIdentifier uniqueId, IdentifierBundle identifiers) : base(name, securityType, uniqueId, identifiers)
        {
        }
    }
}