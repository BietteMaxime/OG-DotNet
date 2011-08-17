//-----------------------------------------------------------------------
// <copyright file="AvailableOutputsResourceTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Engine.View.Helper;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class AvailableOutputsResourceTest : ViewTestsBase
    {
        [Theory]
        [TypedPropertyData("FastTickingViewDefinitions")]
        public void CanGetOutputs(ViewDefinition defn)
        {
            UniqueId portfolio = defn.PortfolioIdentifier;
            if (portfolio == null)
            {
                return;
            }
            Context.RemoteAvailableOutputs.MaxNodes = 100;
            Context.RemoteAvailableOutputs.MaxPositions = 100;
            var availableOutputs = Context.RemoteAvailableOutputs.GetPortfolioOutputs(portfolio);
            Assert.NotNull(availableOutputs);
            foreach (var securityType in availableOutputs.SecurityTypes)
            {
                ICollection<AvailableOutput> positionOutputs = availableOutputs.GetPositionOutputs(securityType);
                Assert.NotEmpty(positionOutputs);
                foreach (var availableOutput in positionOutputs)
                {
                    Assert.Null(availableOutput.PortfolioNodeProperties);
                    Assert.True(new[] {securityType}.SequenceEqual(availableOutput.PositionProperties.Keys));
                    ValueProperties positionProperty = availableOutput.PositionProperties[securityType];
                    Assert.NotNull(positionProperty);
                    Assert.NotNull(positionProperty.GetValues("Function"));
                }
            }

            ICollection<AvailableOutput> portfolioNodeOutputs = availableOutputs.GetPortfolioNodeOutputs();
            Assert.NotNull(portfolioNodeOutputs);
            foreach (var availableOutput in portfolioNodeOutputs)
            {
                Assert.Empty(availableOutput.PositionProperties);
                ValueProperties properties = availableOutput.PortfolioNodeProperties;
                Assert.NotNull(properties);
                Assert.NotNull(properties.GetValues("Function"));
            }
        }

        [Xunit.Extensions.Fact]
        public void UberView()
        {
            var defn = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(" Mixed View");
            UniqueId portfolio = defn.PortfolioIdentifier;

            var remoteAvailableOutputs = Context.RemoteAvailableOutputs;
            remoteAvailableOutputs.MaxNodes = -1;
            remoteAvailableOutputs.MaxPositions = -1;

            var availableOutputs = remoteAvailableOutputs.GetPortfolioOutputs(portfolio);

            Assert.NotNull(availableOutputs);

            var valueNames = availableOutputs.SecurityTypes.ToDictionary(s => s, s => availableOutputs.GetPositionOutputs(s).Select(a => a.ValueName));
            foreach (var securityType in availableOutputs.SecurityTypes)
            {
                ICollection<AvailableOutput> positionOutputs = availableOutputs.GetPositionOutputs(securityType);
                foreach (var availableOutput in positionOutputs)
                {
                    Assert.Null(availableOutput.PortfolioNodeProperties);
                    Assert.True(new[] { securityType }.SequenceEqual(availableOutput.PositionProperties.Keys));
                    ValueProperties positionProperty = availableOutput.PositionProperties[securityType];
                    Assert.NotNull(positionProperty);
                    Assert.NotNull(positionProperty.GetValues("Function"));
                }
            }

            var viewCalculationConfiguration = GetDefaultCalculations(valueNames);
            var newDefn = new ViewDefinition(TestUtils.GetUniqueName(), portfolioIdentifier: portfolio, defaultCurrency: defn.DefaultCurrency, calculationConfigurationsByName: new Dictionary<string, ViewCalculationConfiguration>() { { "Default", viewCalculationConfiguration } });

            using (var remoteClient = Context.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(newDefn));
                RemoteViewCycleTests.WithViewCycle(delegate(ViewDefinitionCompiledArgs comp, IViewCycle cycle, RemoteViewClient client)
                {
                    var allLiveData = cycle.GetResultModel().AllLiveData;
                    var dependencyGraphExplorer = cycle.GetCompiledViewDefinition().GetDependencyGraphExplorer("Default");
                    var dependencyGraph = dependencyGraphExplorer.GetWholeGraph();
                    var dependencyNodes = dependencyGraph.DependencyNodes;
                    var valueSpecifications = dependencyNodes.SelectMany(n => n.OutputValues).ToLookup(s => s).Select(g => g.Key).ToList();

                    var first = cycle.QueryComputationCaches(new ComputationCacheQuery("Default", valueSpecifications)); 
                    Assert.InRange(allLiveData.Count(), 1, valueSpecifications.Count);
                    Assert.InRange(first.Results.Count, allLiveData.Count() + 1, valueSpecifications.Count);
                }, defn.Name);
            }
        }

        private static ViewCalculationConfiguration GetDefaultCalculations( Dictionary<string, IEnumerable<string>> valueNames)
        {
            Dictionary<string, HashSet<Tuple<string, ValueProperties>>> portfolioRequirementsBySecurityType = valueNames.ToDictionary(k => k.Key, k => new HashSet<Tuple<string, ValueProperties>>(k.Value.Select(v => Tuple.Create(v, ValueProperties.Create()))));
            return new ViewCalculationConfiguration("Default", new ValueRequirement[] { }, portfolioRequirementsBySecurityType);
        }
    }
}
