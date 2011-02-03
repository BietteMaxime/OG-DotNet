namespace OGDotNet.Mappedtypes.engine
{
    public enum ComputationTargetType
    {

        /**
         * A set of positions (a portfolio node, or whole portfolio).
         */
        PORTFOLIO_NODE,
        /**
         * A position.
         */
        POSITION,
        /**
         * A security.
         */
        SECURITY,
        /**
         * A simple type, effectively "anything else".
         */
        PRIMITIVE,
        /**
         * A trade.
         */
        TRADE

    }
}