//-----------------------------------------------------------------------
// <copyright file="CompiledViewDefinitionExtensions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

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
