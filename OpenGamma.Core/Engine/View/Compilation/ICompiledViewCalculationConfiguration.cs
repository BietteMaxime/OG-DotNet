// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompiledViewCalculationConfiguration.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Fudge;
using OpenGamma.Util.Tuple;

namespace OpenGamma.Engine.View.Compilation
{
    [FudgeSurrogate(typeof(CompiledViewCalculationConfigurationBuilder))]
    public interface ICompiledViewCalculationConfiguration
    {
        string Name { get; }
        Dictionary<ValueSpecification, HashSet<ValueRequirement>> TerminalOutputSpecifications { get; }
        HashSet<Pair<string, ValueProperties>> TerminalOutputValues { get; }
        Dictionary<ValueRequirement, ValueSpecification> MarketDataRequirements { get; }
        HashSet<ComputationTargetSpecification> ComputationTargets { get; }
    }
}