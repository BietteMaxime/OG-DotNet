using System;

namespace OGDotNet.Builders
{
    static class EnumUtils
    {
        public static TB ConvertTo<TB>(this object a) where TB : struct
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