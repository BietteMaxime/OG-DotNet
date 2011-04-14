//-----------------------------------------------------------------------
// <copyright file="EnumBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace OGDotNet.Builders
{
    static class EnumBuilder<T> where T : struct
    {
        internal static T Parse(string str)
        {
            T type;
            if (!Enum.TryParse(str.Replace("_", string.Empty), true, out type))
            {
                throw new ArgumentException("Unhandled computation target type");
            }
            return type;
        }

        internal static string GetJavaName(T value)
        {
            var netName = value.ToString();
            Regex humpExp = new Regex("([a-z])([A-Z])");
            var javaName = humpExp.Replace(netName, "$1_$2").ToUpperInvariant();
            return javaName;
        }
    }
}
