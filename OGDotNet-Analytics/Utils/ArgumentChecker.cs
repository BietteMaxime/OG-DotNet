//-----------------------------------------------------------------------
// <copyright file="ArgumentChecker.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Utils
{
    internal static class ArgumentChecker
    {
        public static void NotEmpty<T>(IEnumerable<T> arg, string argName)
        {
            NotNull(arg, argName);
            if (!arg.Any())
            {
                throw new ArgumentException("Cannot be empty", argName);
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