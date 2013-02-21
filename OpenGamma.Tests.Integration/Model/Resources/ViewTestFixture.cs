// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewTestFixture.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine.View;
using OpenGamma.Financial.User;
using OpenGamma.Id;
using OpenGamma.LiveData;
using OpenGamma.Master.Config;
using OpenGamma.Master.Portfolio;
using OpenGamma.Master.Position;
using OpenGamma.Master.Security;
using OpenGamma.Model.Context;

namespace OpenGamma.Model.Resources
{
    /// <summary>
    /// Provides access to view definitions and portfolios which may be useful for testing the core remote interfaces.
    /// </summary>
    public class ViewTestFixture : IDisposable
    {
        private readonly FinancialClient _financialClient;

        public ViewTestFixture()
        {
            RemoteEngineContext context = RemoteEngineContextFactoryTests.GetContextFactory().CreateRemoteEngineContext();
            _financialClient = context.CreateFinancialClient();
            EquityViewDefinition = CreateEquityViewDefinition(_financialClient);
        }

        public void Dispose()
        {
            _financialClient.Dispose();
        }

        public ViewDefinition EquityViewDefinition { get; private set; }

        public FinancialClient FinancialClient
        {
            get { return _financialClient; }
        }

        private static ViewDefinition CreateEquityViewDefinition(FinancialClient financialClient)
        {
            var root = new ManageablePortfolioNode("Equities");
            root.AddPosition(CreateEquityPosition("AAPL US Equity", 300, financialClient));
            root.AddPosition(CreateEquityPosition("VOD LN Equity", 250, financialClient));
            var portfolio = new ManageablePortfolio("Sample Portfolio", root);
            var portfolioDoc = new PortfolioDocument(portfolio);
            portfolioDoc = financialClient.PortfolioMaster.Add(portfolioDoc);

            var viewDefinition = new ViewDefinition("Test Equities View", portfolioDoc.UniqueId, UserPrincipal.DefaultUser);
            var calcConfig = new ViewCalculationConfiguration("Default");
            calcConfig.AddPortfolioRequirement("EQUITY", "Value");
            viewDefinition.CalculationConfigurationsByName.Add("Default", calcConfig);

            var viewDefinitionItem = ConfigItem.Create(viewDefinition, viewDefinition.Name);
            var viewDefinitionDoc = new ConfigDocument<ViewDefinition>(viewDefinitionItem);
            viewDefinitionDoc = financialClient.ConfigMaster.Add(viewDefinitionDoc);
            return viewDefinitionDoc.Config.Value;
        }

        private static UniqueId CreateEquityPosition(string ticker, int quantity, FinancialClient financialClient)
        {
            var position = new ManageablePosition
                                     {
                                         SecurityLink = ManageableSecurityLink.Create(ExternalIdBundle.Create("BLOOMBERG_TICKER", ticker)),
                                         Quantity = quantity
                                     };
            var doc = new PositionDocument(position);
            return financialClient.PositionMaster.Add(doc).UniqueId;
        }
    }
}
