using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Mappedtypes.Core.Security
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class Security
    {
        public string Name { get; set; }
        public string SecurityType { get; set; }
        public UniqueIdentifier UniqueId { get; set; }
        public IdentifierBundle Identifiers { get; set; }

        public override string ToString()
        {
            return SecurityType + ": " + Name;
        }
    }
}