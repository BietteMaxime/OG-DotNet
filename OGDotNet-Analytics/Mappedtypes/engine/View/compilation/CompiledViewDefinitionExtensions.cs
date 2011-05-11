using System;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public static class CompiledViewDefinitionExtensions
    {
        public static bool IsValidFor(this ICompiledViewDefinition defn, DateTimeOffset valuationTime)
        {
            return
                (defn.EarliestValidity == default(DateTimeOffset) || defn.EarliestValidity < valuationTime) &&
                (defn.LatestValidity == default(DateTimeOffset) || defn.LatestValidity > valuationTime);
        }
    }
}
