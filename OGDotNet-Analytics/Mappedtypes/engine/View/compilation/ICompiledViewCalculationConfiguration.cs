//-----------------------------------------------------------------------
// <copyright file="ICompiledViewCalculationConfiguration.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.value;

namespace OGDotNet.Mappedtypes.Engine.View.compilation
{
    public interface ICompiledViewCalculationConfiguration
    {
        string Name { get; }
        Dictionary<ValueRequirement, ValueSpecification> MarketDataRequirements { get; }
        HashSet<ComputationTarget> ComputationTargets { get; }
        HashSet<ValueSpecification> TerminalOutputSpecifications { get; }
    }
}