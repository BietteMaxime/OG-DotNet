using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public interface ICompiledViewCalculationConfiguration
    {
        string Name { get; }
        Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements { get; }
        //TODO: targets, terminals...
    }

    class CompiledViewCalculationConfigurationImpl : ICompiledViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly Dictionary<ValueRequirement, ValueSpecification> _liveDataRequirements;

        public CompiledViewCalculationConfigurationImpl(string name, Dictionary<ValueRequirement, ValueSpecification> liveDataRequirements)
        {
            _name = name;
            _liveDataRequirements = liveDataRequirements;
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements
        {
            get { return _liveDataRequirements; }
        }
    }
}