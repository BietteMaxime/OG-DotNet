// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvailableOutputsResourceTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Calc;
using OpenGamma.Engine.View.Helper;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Financial.view.rest;
using OpenGamma.Id;
using OpenGamma.Master.Config;
using OpenGamma.Xunit.Extensions;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class AvailableOutputsResourceTest : ViewTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetOutputs()
        {
            UniqueId portfolio = Fixture.EquityViewDefinition.PortfolioId;
            var remoteAvailableOutputs = Context.RemoteAvailableOutputs;
            remoteAvailableOutputs.MaxNodes = 2;
            remoteAvailableOutputs.MaxPositions = 2;
            var availableOutputs = remoteAvailableOutputs.GetPortfolioOutputs(portfolio);
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
            var defn = Context.ViewProcessor.ConfigSource.Get<ViewDefinition>("Mixed Instrument VaR View");
            UniqueId portfolio = defn.PortfolioId;

            var remoteAvailableOutputs = Context.RemoteAvailableOutputs;
            remoteAvailableOutputs.MaxNodes = 2;
            remoteAvailableOutputs.MaxPositions = 2;

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
            var newDefn = new ViewDefinition(TestUtils.GetUniqueName(), portfolioId: portfolio, defaultCurrency: defn.DefaultCurrency, calculationConfigurationsByName: new Dictionary<string, ViewCalculationConfiguration> { { "Default", viewCalculationConfiguration } });

            using (var financialClient = Context.CreateFinancialClient())
            {
                var item = ConfigItem.Create(newDefn, newDefn.Name);
                var doc = new ConfigDocument<ViewDefinition>(item);
                financialClient.ConfigMaster.Add(doc);
                RemoteViewCycleTests.WithViewCycle(delegate(ViewDefinitionCompiledArgs comp, IViewCycle cycle, RemoteViewClient client)
                {
                    var allLiveData = cycle.GetResultModel().AllLiveData;
                    var dependencyGraphExplorer = cycle.GetCompiledViewDefinition().GetDependencyGraphExplorer("Default");
                    var dependencyGraph = dependencyGraphExplorer.GetWholeGraph();
                    var dependencyNodes = dependencyGraph.DependencyNodes;
                    var valueSpecifications = dependencyNodes.SelectMany(n => n.OutputValues).ToLookup(s => s).Select(g => g.Key).ToList();
                    Assert.NotEmpty(valueSpecifications);
                    var first = cycle.QueryComputationCaches(new ComputationCacheQuery("Default", valueSpecifications)); 
                    Assert.InRange(allLiveData.Count(), 1, valueSpecifications.Count);
                    Assert.InRange(first.Results.Count, allLiveData.Count() + 1, valueSpecifications.Count);
                }, Fixture.EquityViewDefinition.UniqueId);
            }
        }

        private static ViewCalculationConfiguration GetDefaultCalculations(Dictionary<string, IEnumerable<string>> valueNames)
        {
            var calcConfig = new ViewCalculationConfiguration("Default");
            foreach (string secType in valueNames.Keys)
            {
                calcConfig.AddPortfolioRequirements(secType, valueNames[secType].Select(v => Tuple.Create(v, ValueProperties.Create())));
            }
            return calcConfig;
        }
    }
}
