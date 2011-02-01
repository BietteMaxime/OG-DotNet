﻿using System.Collections.ObjectModel;

using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Mappedtypes.Core.Security
{
    /**
* A source of security information as accessed by the main application.
* <p>
* This interface provides a simple view of securities as needed by the engine.
* This may be backed by a full-featured security master, or by a much simpler data structure.
*/
    public interface ISecuritySource
    {

        /**
         * Finds a specific security by unique identifier.
         * <p>
         * Since a unique identifier is unique, there are no complex matching issues.
         * 
         * @param uid  the unique identifier, null returns null
         * @return the security, null if not found
         * @throws IllegalArgumentException if the identifier is invalid
         */
        Security GetSecurity(UniqueIdentifier uid);

        /**
         * Finds all securities that match the specified bundle of keys.
         * <p>
         * The identifier bundle represents those keys associated with a single security.
         * In an ideal world, all the identifiers in a bundle would refer to the same security.
         * However, since each identifier is not completely unique, multiple may match.
         * To further complicate matters, some identifiers are more unique than others.
         * <p>
         * The simplest implementation of this method will return a security if it matches one of the keys.
         * A more advanced implementation will choose using some form of priority order which
         * key or keys from the bundle to search for.
         * 
         * @param bundle  the bundle keys to match, not null
         * @return all securities matching the specified key, empty if no matches, not null
         * @throws IllegalArgumentException if the identifier is invalid
         */
        Collection<Security> getSecurities(IdentifierBundle bundle);

        /**
         * Finds the single best-fit security that matches the specified bundle of keys.
         * <p>
         * The identifier bundle represents those keys associated with a single security.
         * In an ideal world, all the identifiers in a bundle would refer to the same security.
         * However, since each identifier is not completely unique, multiple may match.
         * To further complicate matters, some identifiers are more unique than others.
         * <p>
         * An implementation will need some mechanism to decide what the best-fit match is.
         * 
         * @param bundle  the bundle keys to match, not null
         * @return the single security matching the bundle of keys, null if not found
         * @throws IllegalArgumentException if the identifier is invalid
         */
        Security GetSecurity(IdentifierBundle bundle);

    }
}