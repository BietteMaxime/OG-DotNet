using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.currency;
using OGDotNet.Mappedtypes.financial.view;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class CurrencyMatrixSourcingFunctionTests : TestWithContextBase
    {
        [Fact]
        public void CanGetIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, Currency.Create("USD"), Currency.Create("USD"));
            Assert.Equal(1.0,conversionRate);
        }
        

        [Fact]
        public void CanGetNonIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, Currency.Create("GBP"), Currency.Create("USD"));
            Assert.NotEqual(1.0, conversionRate);
        }

        private CurrencyMatrixSourcingFunction GetFunction()
        {
            var currencyMatrix = Context.CurrencyMatrixSource.GetCurrencyMatrix("BloombergLiveData");
            return new CurrencyMatrixSourcingFunction(currencyMatrix);
        }

        private object GetValue(ValueRequirement req)
        {
            if (req.TargetSpecification.Type != ComputationTargetType.Primitive)
                throw new NotImplementedException();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using (var remoteClient = Context.CreateUserClient())
            {
                var viewDefinition = new ViewDefinition(Guid.NewGuid().ToString());

                var viewCalculationConfiguration = new ViewCalculationConfiguration("Default", new List<ValueRequirement> { req }, new Dictionary<string, ValueProperties>());
                viewDefinition.CalculationConfigurationsByName.Add("Default", viewCalculationConfiguration);
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));

                var remoteView = Context.ViewProcessor.GetView(viewDefinition.Name);
                remoteView.Init();
                var remoteViewClient = remoteView.CreateClient();
                foreach (var viewComputationResultModel in remoteViewClient.GetResults(cancellationTokenSource.Token))
                {
                    foreach (var val in viewComputationResultModel.AllResults)
                    {
                        Debug.Assert(val.CalculationConfiguration == "Default");
                        Debug.Assert(req.IsSatisfiedBy(val.ComputedValue.Specification));
                        return val.ComputedValue.Value;
                    }
                }
            }
            throw new NotImplementedException();
        }
    }
}