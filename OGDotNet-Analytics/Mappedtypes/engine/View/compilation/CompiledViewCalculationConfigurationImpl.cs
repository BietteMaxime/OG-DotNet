//-----------------------------------------------------------------------
// <copyright file="CompiledViewCalculationConfigurationImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    class CompiledViewCalculationConfigurationImpl : ICompiledViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly Dictionary<ValueRequirement, ValueSpecification> _liveDataRequirements;
        private readonly HashSet<ComputationTarget> _computationTargets;
        private readonly HashSet<ValueSpecification> _terminalOutputSpecifications;

        public CompiledViewCalculationConfigurationImpl(string name, Dictionary<ValueRequirement, ValueSpecification> liveDataRequirements, HashSet<ComputationTarget> computationTargets, HashSet<ValueSpecification> terminalOutputSpecifications)
        {
            _name = name;
            _liveDataRequirements = liveDataRequirements;
            _computationTargets = computationTargets;
            _terminalOutputSpecifications = terminalOutputSpecifications;
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements
        {
            get { return _liveDataRequirements; }
        }

        public HashSet<ComputationTarget> ComputationTargets
        {
            get { return _computationTargets; }
        }

        public HashSet<ValueSpecification> TerminalOutputSpecifications
        {
            get { return _terminalOutputSpecifications; }
        }

        public static CompiledViewCalculationConfigurationImpl FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new CompiledViewCalculationConfigurationImpl(ffc.GetString("name"), MapBuilder.FromFudgeMsg<ValueRequirement, ValueSpecification>(ffc.GetMessage("liveDataRequirements"), deserializer), new HashSet<ComputationTarget>(ffc.GetMessage("computationTargets").GetAllByOrdinal(1).Select(deserializer.FromField<ComputationTarget>)),
                new HashSet<ValueSpecification>(ffc.GetMessage("terminalOutputSpecifications").GetAllByOrdinal(1).Select(deserializer.FromField<ValueSpecification>)));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }   
    }
}