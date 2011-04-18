using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public class CompiledViewDefinitionImpl : ICompiledViewDefinition
    {
        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly Dictionary<ValueRequirement, ValueSpecification> _liveDataRequirements;
        private readonly IList<string> _outputValueNames;
        private readonly IList<string> _securityTypes;

        public CompiledViewDefinitionImpl(ViewDefinition viewDefinition, IPortfolio portfolio, Dictionary<ValueRequirement, ValueSpecification> liveDataRequirements, IList<string> outputValueNames, IList<string> securityTypes)
        {
            _viewDefinition = viewDefinition;
            _portfolio = portfolio;
            _liveDataRequirements = liveDataRequirements;
            _outputValueNames = outputValueNames;
            _securityTypes = securityTypes;
        }

        public ViewDefinition ViewDefinition
        {
            get { return _viewDefinition; }
        }

        public IPortfolio Portfolio
        {
            get { return _portfolio; }
        }

        public Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements
        {
            get { return _liveDataRequirements; }
        }

        public IList<string> OutputValueNames
        {
            get { return _outputValueNames; }
        }

        public IList<string> SecurityTypes
        {
            get { return _securityTypes; }
        }
    }
}
