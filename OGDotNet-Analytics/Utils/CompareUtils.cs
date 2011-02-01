using System;

namespace OGDotNet_Analytics.Utils
{
    public static class CompareUtils
    {
        public static int CompareWithNull<T>(IComparable<T> a, T b) where T : class
        {
            if (a == null) {
                return b == null ? 0 : -1;
            } else if (b == null) {
                return 1;  // a not null
            } else {
                return a.CompareTo(b);
            }    
        }
    }
}