// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestViewUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Financial.User;
using OpenGamma.Master.Config;
using OpenGamma.Xunit.Extensions;

namespace OpenGamma.Model.Resources
{
    public static class TestViewUtils
    {
        public static ViewDefinition CreateViewDefinition(FinancialClient financialClient, ValueRequirement valueRequirement)
        {
            var calcConfig = new ViewCalculationConfiguration("Default");
            calcConfig.AddSpecificRequirement(valueRequirement);
            var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());
            viewDefinition.AddCalculationConfiguration(calcConfig);
            var configItem = ConfigItem.Create(viewDefinition, viewDefinition.Name);
            var doc = new ConfigDocument<ViewDefinition>(configItem);
            doc = financialClient.ConfigMaster.Add(doc);
            return doc.Config.Value;
        }
    }
}