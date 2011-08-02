//-----------------------------------------------------------------------
// <copyright file="DependencyNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.Function;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Mappedtypes.Engine.DepGraph
{
    [FudgeSurrogate(typeof(DependencyNodeBuilder))]
    public class DependencyNode
    {
        private readonly IEnumerable<ValueSpecification> _inputValues;
        private readonly IEnumerable<ValueSpecification> _outputValues;
        private readonly IEnumerable<ValueSpecification> _terminalOutputValues;
        private readonly ParameterizedFunction _function;
        private readonly ComputationTarget _target;
        private readonly ICollection<DependencyNode> _inputNodes = new List<DependencyNode>();

        public DependencyNode(ComputationTarget target, IEnumerable<ValueSpecification> inputValues, IEnumerable<ValueSpecification> outputValues, IEnumerable<ValueSpecification> terminalOutputValues, ParameterizedFunction function)
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
    }
}