//-----------------------------------------------------------------------
// <copyright file="RemoteCompiledViewDefinitionWithGraphs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.javax.time;

namespace OGDotNet.Model.Resources
{
    internal class RemoteCompiledViewDefinitionWithGraphs : ICompiledViewDefinitionWithGraphs
    {
        private readonly RestTarget _rest;

        public RemoteCompiledViewDefinitionWithGraphs(RestTarget rest)
        {
            _rest = rest;
        }

        public ViewDefinition ViewDefinition
        {
            get { return _rest.Resolve("viewDefinition").Get<ViewDefinition>(); }
        }

        public IPortfolio Portfolio
        {
            get { return _rest.Resolve("portfolio").Get<IPortfolio>(); }
        }

        public Dictionary<ValueRequirement, ValueSpecification> MarketDataRequirements
        {
            get { return _rest.Resolve("marketDataRequirements").Get<Dictionary<ValueRequirement, ValueSpecification>>(); }
        }

        public DateTimeOffset EarliestValidity
        {
            get { return _rest.Resolve("validFrom").Get<Instant>().ToDateTimeOffset(); }
        }

        public DateTimeOffset LatestValidity
        {
            get { return _rest.Resolve("validTo").Get<Instant>().ToDateTimeOffset(); }
        }

        public Dictionary<string, ICompiledViewCalculationConfiguration> CompiledCalculationConfigurations
        {
            get
            {
                return _rest.Resolve("compiledCalculationConfigurations").Get<List<ICompiledViewCalculationConfiguration>>().ToDictionary(c => c.Name);
            }
        }

        public IDependencyGraphExplorer GetDependencyGraphExplorer(string calcConfig)
        {
            return new RemoteDependencyGraphExplorer(_rest.Resolve("graphs").Resolve(calcConfig));
        }
    }
}