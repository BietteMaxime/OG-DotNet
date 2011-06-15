//-----------------------------------------------------------------------
// <copyright file="IViewResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public interface IViewResultModel
    {
        DateTimeOffset ValuationTime { get; }
        DateTimeOffset ResultTimestamp { get; }
        UniqueIdentifier ViewProcessId { get; }
        UniqueIdentifier ViewCycleId { get; }
        IEnumerable<ViewResultEntry> AllResults { get; }
        ComputedValue this[string calculationConfiguration, ValueRequirement valueRequirement] { get; }
        bool TryGetValue(string calculationConfiguration, ValueRequirement valueRequirement, out object result);
        bool TryGetComputedValue(string calculationConfiguration, ValueRequirement valueRequirement, out ComputedValue result);
    }
}