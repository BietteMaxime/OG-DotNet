namespace OGDotNet.Mappedtypes.Id
{
    public enum IdentifierSearchType
    {
        /**
        * Match requires that the target must contain exactly the same set of identifiers.
        */
        EXACT,
        /**
         * Match requires that the target must contain all of the search identifiers.
         */
        ALL,
        /**
         * Match requires that the target must contain any of the search identifiers.
         */
        ANY,
        /**
         * Match requires that the target must contain none of the search identifiers.
         */
        NONE
    }
}