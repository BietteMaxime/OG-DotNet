// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrencyMatrixSourcingFunctionTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine.Target;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Financial.Ccy;
using OpenGamma.Master.Config;
using OpenGamma.Util.Money;
using OpenGamma.Xunit.Extensions;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class CurrencyMatrixSourcingFunctionTests : RemoteEngineContextTestBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, Currency.USD, Currency.USD);
            Assert.Equal(1.0, conversionRate);
        }

        [Xunit.Extensions.Fact]
        public void CanGetNonIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var source = Currency.USD;
            var target = Currency.GBP;

            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, source, target);
            Assert.NotEqual(1.0, conversionRate);
            var reciprocal = currencyMatrixSourcingFunction.GetConversionRate(GetValue, target, source);
            Assert.NotEqual(1.0, reciprocal);

            Assert.InRange(conversionRate, 0.99 / reciprocal, 1.01 / reciprocal);
        }

        private static CurrencyMatrixSourcingFunction GetFunction()
        {
            var currencyMatrix = Context.CurrencyMatrixSource.GetCurrencyMatrix("BloombergLiveData");
            Assert.NotNull(currencyMatrix);
            return new CurrencyMatrixSourcingFunction(currencyMatrix);
        }

        private static double GetValue(ValueRequirement req)
        {
            if (req.TargetReference.Type != ComputationTargetType.Primitive)
                throw new NotImplementedException();

            using (var financialClient = Context.CreateFinancialClient())
            {
                var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());

                var calcConfig = new ViewCalculationConfiguration("Default");
                calcConfig.AddSpecificRequirement(req);
                viewDefinition.AddCalculationConfiguration(calcConfig);
                var item = ConfigItem.Create(viewDefinition, viewDefinition.Name);
                var doc = new ConfigDocument<ViewDefinition>(item);
                doc = financialClient.ConfigMaster.Add(doc);
                viewDefinition.UniqueId = doc.UniqueId;
                using (var viewClient = Context.ViewProcessor.CreateViewClient())
                {
                    var viewComputationResultModel = viewClient.GetResults(viewDefinition.UniqueId, ExecutionOptions.SingleCycle).First();
                    return (double)viewComputationResultModel["Default", req].Value;
                }
            }
        }
    }
}