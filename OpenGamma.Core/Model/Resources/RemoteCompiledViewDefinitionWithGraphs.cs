// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteCompiledViewDefinitionWithGraphs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using OpenGamma.Core.Position;
using OpenGamma.Engine.DepGraph;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Compilation;
using OpenGamma.Financial.view.rest;
using OpenGamma.Time;

namespace OpenGamma.Model.Resources
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
            get
            {
                var validFrom = _rest.Resolve("validFrom").Get<Instant>();
                return validFrom != null ? validFrom.ToDateTimeOffset() : default(DateTimeOffset);
            }
        }

        public DateTimeOffset LatestValidity
        {
            get
            {
                var validTo = _rest.Resolve("validTo").Get<Instant>();
                return validTo != null ? validTo.ToDateTimeOffset() : default(DateTimeOffset);
            }
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