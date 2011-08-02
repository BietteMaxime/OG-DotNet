//-----------------------------------------------------------------------
// <copyright file="DependencyNodeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.Function;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Builders
{
    class DependencyNodeBuilder : BuilderBase<DependencyNode>
    {
        public DependencyNodeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override DependencyNode DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var target = deserializer.FromField<ComputationTarget>(ffc.GetByName("target"));

            string parameterizedFunctionUniqueId = ffc.GetString("parameterizedFunctionUniqueId");
            var functionParametersField = ffc.GetByName("functionParameters");
            var functionParameters = functionParametersField != null ? deserializer.FromField<IFunctionParameters>(functionParametersField) : null;

            string functionShortName = ffc.GetString("functionShortName");
            string functionUniqueId = ffc.GetString("functionUniqueId");

            ICompiledFunctionDefinition function = new CompiledFunctionDefinitionStub(target.Type, functionShortName, functionUniqueId);
            var parameterizedFunction = new ParameterizedFunction(function, functionParameters, parameterizedFunctionUniqueId);

            var inputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("inputValues"));
            var outputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("outputValues"));
            var terminalOutputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("terminalOutputValues"));

            return new DependencyNode(target, inputValues, outputValues, terminalOutputValues, parameterizedFunction);
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
