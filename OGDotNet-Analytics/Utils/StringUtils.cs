//-----------------------------------------------------------------------
// <copyright file="StringUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Utils
{
    internal static class StringUtils
    {
        public static string TrimToNull(string version)
        {
            switch (version)
            {
                case null:
                case "":
                    return null;
                default:
                    return version;
            }
        }
    }
}