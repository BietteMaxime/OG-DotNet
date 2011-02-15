using System;
using System.Text.RegularExpressions;
using OGDotNet.Mappedtypes.engine;

namespace OGDotNet.Builders
{
    static class ComputationTargetTypeBuilder
    {
        internal static ComputationTargetType GetComputationTargetType(string str)
        {
            ComputationTargetType type;
            if (! Enum.TryParse(str.Replace("_",""), true, out type))
            {
                throw new ArgumentException("Unhandled computation target type");
            }
            return type;
        }
        internal static string GetJavaName(ComputationTargetType type)
        {
            var netName = type.ToString();
            Regex humpExp = new Regex("([a-z])([A-Z])");
            var javaName = humpExp.Replace(netName, "$1_$2").ToUpper();
            return javaName;
        }
    }
}
