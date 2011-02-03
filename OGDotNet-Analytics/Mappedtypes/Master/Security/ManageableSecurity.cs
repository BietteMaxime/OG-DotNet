using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Security
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class ManageableSecurity : Core.Security.Security
    {
        public ManageableSecurity(string name, string securityType, UniqueIdentifier uniqueId, IdentifierBundle identifiers) : base(name, securityType, uniqueId, identifiers)
        {
        }
    }
}