using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Utils
{
    internal static class ArgumentChecker
    {
        public static void NotEmpty<T>(IEnumerable<T> arg, string argName)
        {
            NotNull(arg,argName);
            if (!arg.Any())
            {
                throw new ArgumentException(argName, "Cannot be empty");
            }
        }

        public static void NotNull<T>(T arg, string argName) where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }
    }
}