// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyNodeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OpenGamma.Engine.DepGraph;
using OpenGamma.Engine.Function;
using OpenGamma.Engine.Target;
using OpenGamma.Engine.Value;

namespace OpenGamma.Fudge
{
    class DependencyNodeBuilder : BuilderBase<DependencyNode>
    {
        public DependencyNodeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override DependencyNode DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var target = deserializer.FromField<ComputationTargetSpecification>(msg.GetByName("target"));

            string parameterizedFunctionUniqueId = msg.GetString("parameterizedFunctionUniqueId");
            var functionParametersField = msg.GetByName("functionParameters");
            var functionParameters = functionParametersField != null ? deserializer.FromField<IFunctionParameters>(functionParametersField) : null;

            string functionShortName = msg.GetString("functionShortName");
            string functionUniqueId = msg.GetString("functionUniqueId");

            ICompiledFunctionDefinition function = new CompiledFunctionDefinitionStub(target.Type, functionShortName, functionUniqueId);
            var parameterizedFunction = new ParameterizedFunction(function, functionParameters, parameterizedFunctionUniqueId);

            var inputValues = DeserializeSet<ValueSpecification>(deserializer, msg, "inputValues");
            var outputValues = DeserializeSet<ValueSpecification>(deserializer, msg, "outputValues");
            var terminalOutputValues = DeserializeSet<ValueSpecification>(deserializer, msg, "terminalOutputValues");

            return new DependencyNode(target, inputValues, outputValues, terminalOutputValues, parameterizedFunction);
        }

        private static HashSet<T> DeserializeSet<T>(IFudgeDeserializer deserializer, IFudgeFieldContainer ffc, string fieldName) where T : class
        {
            return new HashSet<T>(ffc.GetMessage(fieldName).GetAllByOrdinal(1).Select(deserializer.FromField<T>));
        }

        private class CompiledFunctionDefinitionStub : ICompiledFunctionDefinition
        {
            private readonly ComputationTargetType _targetType;
            private readonly FunctionDefinitionStub _functionDefinition;

            public CompiledFunctionDefinitionStub(ComputationTargetType targetType, string uniqueId, string shortName)
            {
                _targetType = targetType;
                _functionDefinition = new FunctionDefinitionStub(uniqueId, shortName);
            }

            public ComputationTargetType TargetType
            {
                get { return _targetType; }
            }

            public IFunctionDefinition FunctionDefinition
            {
                get { return _functionDefinition; }
            }
        }

        private class FunctionDefinitionStub : IFunctionDefinition
        {
            private readonly string _uniqueId;
            private readonly string _shortName;

            public FunctionDefinitionStub(string uniqueId, string shortName)
            {
                _uniqueId = uniqueId;
                _shortName = shortName;
            }

            public string UniqueId
            {
                get { return _uniqueId; }
            }

            public string ShortName
            {
                get { return _shortName; }
            }
        }
    }
}
