//-----------------------------------------------------------------------
// <copyright file="CompiledViewCalculationConfigurationImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
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