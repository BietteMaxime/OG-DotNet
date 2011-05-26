//-----------------------------------------------------------------------
// <copyright file="DependencyNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.function;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.depgraph
{
    public class DependencyNode
    {
        private readonly IEnumerable<ValueSpecification> _inputValues;
        private readonly IEnumerable<ValueSpecification> _outputValues;
        private readonly IEnumerable<ValueSpecification> _terminalOutputValues;
        private readonly ParameterizedFunction _function;
        private readonly ComputationTarget _target;
        private readonly ICollection<DependencyNode> _inputNodes = new List<DependencyNode>();

        private DependencyNode(ComputationTarget target, IEnumerable<ValueSpecification> inputValues, IEnumerable<ValueSpecification> outputValues, IEnumerable<ValueSpecification> terminalOutputValues, ParameterizedFunction function)
        {
            _target = target;
            _inputValues = inputValues;
            _outputValues = outputValues;
            _terminalOutputValues = terminalOutputValues;
            _function = function;
        }

        public ComputationTarget Target
        {
            get { return _target; }
        }

        public IEnumerable<DependencyNode> InputNodes
        {
            get { return _inputNodes; }
        }

        public IEnumerable<ValueSpecification> InputValues
        {
            get { return _inputValues; }
        }

        public IEnumerable<ValueSpecification> OutputValues
        {
            get { return _outputValues; }
        }

        public IEnumerable<ValueSpecification> TerminalOutputValues
        {
            get { return _terminalOutputValues; }
        }

        public ParameterizedFunction Function
        {
            get { return _function; }
        }

        public void AddInputNode(DependencyNode inputNode)
        {
            _inputNodes .Add(inputNode);
        }

        public static DependencyNode FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var target = deserializer.FromField<ComputationTarget>(ffc.GetByName("target"));

            string functionUniqueId = ffc.GetString("functionUniqueId");
            var functionParametersField = ffc.GetByName("functionParameters");
            var functionParameters = functionParametersField != null ? deserializer.FromField<IFunctionParameters>(functionParametersField) : null;

            ICompiledFunctionDefinition function = new CompiledFunctionDefinitionStub(target.Type);
            var parameterizedFunction = new ParameterizedFunction(function, functionParameters, functionUniqueId);

            var inputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("inputValues"));
            var outputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("outputValues"));
            var terminalOutputValues = deserializer.FromField<ValueSpecification[]>(ffc.GetByName("terminalOutputValues"));

            return new DependencyNode(target, inputValues, outputValues, terminalOutputValues, parameterizedFunction);
        }

        private class CompiledFunctionDefinitionStub : ICompiledFunctionDefinition
        {
            private readonly ComputationTargetType _targetType;

            public CompiledFunctionDefinitionStub(ComputationTargetType targetType)
            {
                _targetType = targetType;
            }

            public ComputationTargetType TargetType
            {
                get { return _targetType; }
            }
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}