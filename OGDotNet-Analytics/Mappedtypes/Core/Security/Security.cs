using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Security
{
    /// <summary>
    /// TODO: kill these with the .proto replacement
    /// </summary>
    public class Security
    {
        private readonly string _name;
        private readonly string _securityType;
        private readonly UniqueIdentifier _uniqueId;
        private readonly IdentifierBundle _identifiers;

        protected Security(string name, string securityType, UniqueIdentifier uniqueId, IdentifierBundle identifiers)
        {
            _name = name;
            _securityType = securityType;
            _uniqueId = uniqueId;
            _identifiers = identifiers;
        }

        public string Name
        {
            get { return _name; }
        }

        public string SecurityType
        {
            get { return _securityType; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public IdentifierBundle Identifiers
        {
            get { return _identifiers; }
        }

        public override string ToString()
        {
            return SecurityType + ": " + Name;
        }
    }
}