using Fudge.Serialization;
using OGDotNet_Analytics.Builders;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Mappedtypes.Core.Position
{
    [FudgeSurrogate(typeof(PositionBuilder))]
    public class Position
    {
        private readonly IdentifierBundle _securityKey;
        private readonly UniqueIdentifier _identifier;
        private readonly long _quantity;

        public Position(UniqueIdentifier identifier, long quantity, IdentifierBundle securityKey)
        {
            _securityKey = securityKey;
            _identifier = identifier;
            _quantity = quantity;
        }

        public IdentifierBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public UniqueIdentifier Identifier
        {
            get { return _identifier; }
        }

        public long Quantity
        {
            get { return _quantity; }
        }
    }
}