//-----------------------------------------------------------------------
// <copyright file="CompiledViewCalculationConfigurationImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Util.Tuple;

namespace OGDotNet.Mappedtypes.Engine.View.Compilation
{
    [FudgeSurrogate(typeof(CompiledViewCalculationConfigurationBuilder))]
    class CompiledViewCalculationConfigurationImpl : ICompiledViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly Dictionary<ValueRequirement, ValueSpecification> _marketDataRequirements;
        private readonly HashSet<ComputationTargetSpecification> _computationTargets;
        private readonly Dictionary<ValueSpecification, HashSet<ValueRequirement>> _terminalOutputSpecifications;
        private readonly HashSet<Pair<string, ValueProperties>> _terminalOutputValues;

        public CompiledViewCalculationConfigurationImpl(string name, Dictionary<ValueRequirement, ValueSpecification> marketDataRequirements, HashSet<ComputationTargetSpecification> computationTargets, Dictionary<ValueSpecification, HashSet<ValueRequirement>> terminalOutputSpecifications)
        {
            _name = name;
            _marketDataRequirements = marketDataRequirements;
            _computationTargets = computationTargets;
            _terminalOutputSpecifications = terminalOutputSpecifications;
            _terminalOutputValues = BuildTerminalOutputValues(terminalOutputSpecifications);
        }

        private static HashSet<Pair<string, ValueProperties>> BuildTerminalOutputValues(Dictionary<ValueSpecification, HashSet<ValueRequirement>> terminalOutputSpecifications)
        {
            var ret = new HashSet<Pair<string, ValueProperties>>();
            foreach (var key in terminalOutputSpecifications.Keys)
            {
                ret.Add(Pair.Create(key.ValueName, key.Properties));
            }
            return ret;
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<ValueSpecification, HashSet<ValueRequirement>> TerminalOutputSpecifications
        {
            get { return _terminalOutputSpecifications; }
        }

        public HashSet<Pair<string, ValueProperties>> TerminalOutputValues
        {
            get { return _terminalOutputValues; }
        }

        public Dictionary<ValueRequirement, ValueSpecification> MarketDataRequirements
        {
            get { return _marketDataRequirements; }
        }

        public HashSet<ComputationTargetSpecification> ComputationTargets
        {
            get { return _computationTargets; }
        }
    }
}