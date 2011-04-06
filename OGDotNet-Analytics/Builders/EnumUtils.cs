using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Builders
{
    public static class EnumUtils<TA,TB> where TA : struct where TB : struct
    {
        private static readonly Dictionary<TA, TB> LookupTable;

        static EnumUtils()
        {
            LookupTable = Enum.GetValues(typeof (TA)).Cast<TA>().ToDictionary(a => a, ConvertToInner);
        }
        public static TB ConvertTo(TA a) 
        {
            return LookupTable[a];
        }

        private static TB ConvertToInner(TA a) 
        {//TODO less slow
            TB ret;
            if (! Enum.TryParse(a.ToString(), out ret))
            {
                throw new ArgumentException();
            }
            return ret;
        }
    }
}