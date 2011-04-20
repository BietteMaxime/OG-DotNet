//-----------------------------------------------------------------------
// <copyright file="CompiledViewDefinitionImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Position;
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
        private readonly string[] _outputValueNames;
        private readonly string[] _securityTypes;
        private readonly DateTimeOffset _latestValidity;
        private readonly DateTimeOffset _earliestValidity;

        public CompiledViewDefinitionImpl(ViewDefinition viewDefinition, IPortfolio portfolio, Dictionary<ValueRequirement, ValueSpecification> liveDataRequirements, string[] outputValueNames, string[] securityTypes, DateTimeOffset latestValidity, DateTimeOffset earliestValidity)
        {
            _viewDefinition = viewDefinition;
            _earliestValidity = earliestValidity;
            _latestValidity = latestValidity;
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

        public string[] OutputValueNames
        {
            get { return _outputValueNames; }
        }

        public string[] SecurityTypes
        {
            get { return _securityTypes; }
        }

        public DateTimeOffset EarliestValidity
        {
            get { return _earliestValidity; }
        }

        public DateTimeOffset LatestValidity
        {
            get { return _latestValidity; }
        }
    }
}
