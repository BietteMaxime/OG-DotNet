using System;
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
    }
}
