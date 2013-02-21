// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenGamma.Fudge
{
    public static class EnumUtils
    {
        public static IEnumerable<T> EnumValues<T>() where T : struct
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("Not an enum type", "T");
            }

            Array values = Enum.GetValues(type);
            return values.Cast<T>();
        }
    }
    public static class EnumUtils<TA, TB>
        where TA : struct
        where TB : struct
    {
        private static readonly Dictionary<TA, TB> LookupTable;

        static EnumUtils()
        {
            var values = EnumUtils.EnumValues<TA>();
            LookupTable = new Dictionary<TA, TB>();
            foreach (var a in values)
            {
                TB b;
                if (Enum.TryParse(a.ToString(), out b))
                {
                    LookupTable.Add(a, b);
                }
            }
        }

        public static TB ConvertTo(TA a)
        {
            return LookupTable[a];
        }
    }
}