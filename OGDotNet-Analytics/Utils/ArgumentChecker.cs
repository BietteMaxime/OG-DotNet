using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet_Analytics.Utils
{
    internal static class ArgumentChecker
    {
        public static void NotEmpty<T>(IEnumerable<T> arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
            if (!arg.Any())
            {
                throw new ArgumentOutOfRangeException(argName, "Cannot be empty");
            }
        }
    }
}