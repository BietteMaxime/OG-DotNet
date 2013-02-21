// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompiledViewCalculationConfigurationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View.Compilation;

namespace OpenGamma.Fudge
{
    class CompiledViewCalculationConfigurationBuilder : BuilderBase<CompiledViewCalculationConfigurationImpl>
    {
        public CompiledViewCalculationConfigurationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override CompiledViewCalculationConfigurationImpl DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string name = msg.GetString("name");
            Dictionary<ValueRequirement, ValueSpecification> marketDataRequirements = MapBuilder.FromFudgeMsg<ValueRequirement, ValueSpecification>(msg.GetMessage("marketDataRequirements"), deserializer);
            var computationTargets = new HashSet<ComputationTargetSpecification>(msg.GetMessage("computationTargets").GetAllByOrdinal(1).Select(deserializer.FromField<ComputationTargetSpecification>));

            IFudgeFieldContainer specMessage = msg.GetMessage("terminalOutputSpecifications");
            Dictionary<ValueSpecification, HashSet<ValueRequirement>> terminalOutputSpecifications = MapBuilder.FromFudgeMsg(specMessage, deserializer.FromField<ValueSpecification>, f => GetRequirementSet(f, deserializer));

            return new CompiledViewCalculationConfigurationImpl(name, marketDataRequirements, computationTargets, terminalOutputSpecifications);
        }

        private static HashSet<ValueRequirement> GetRequirementSet(IFudgeField field, IFudgeDeserializer deserializer)
        {
            var msg = (IFudgeFieldContainer) field.Value;
            return new HashSet<ValueRequirement>(msg.GetAllByOrdinal(1).Select(deserializer.FromField<ValueRequirement>));
        }
    }
}
