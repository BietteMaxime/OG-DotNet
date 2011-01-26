using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet
{
    internal static class ArgumentChecker
    {
        public static void NotEmpty<T>(IEnumerable<T> arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentException("Cannot be null", argName);
            }
            if (!arg.Any())
            {
                throw new ArgumentException("Cannot be empty", argName);
            }
        }
    }
}