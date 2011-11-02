//-----------------------------------------------------------------------
// <copyright file="CompiledViewCalculationConfigurationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View.Compilation;

namespace OGDotNet.Builders
{
    class CompiledViewCalculationConfigurationBuilder : BuilderBase<CompiledViewCalculationConfigurationImpl>
    {
        public CompiledViewCalculationConfigurationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override CompiledViewCalculationConfigurationImpl DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new CompiledViewCalculationConfigurationImpl(msg.GetString("name"), MapBuilder.FromFudgeMsg<ValueRequirement, ValueSpecification>(msg.GetMessage("marketDataRequirements"), deserializer), new HashSet<ComputationTarget>(msg.GetMessage("computationTargets").GetAllByOrdinal(1).Select(deserializer.FromField<ComputationTarget>)),
                new HashSet<ValueSpecification>(msg.GetMessage("terminalOutputSpecifications").GetAllByOrdinal(1).Select(deserializer.FromField<ValueSpecification>)));
        }
    }
}
