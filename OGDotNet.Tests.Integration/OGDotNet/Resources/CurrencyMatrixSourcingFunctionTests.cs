using System;
using System.Linq;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.currency;
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

        private static object GetValue(ValueRequirement arg)
        {
            return 2.0;
        }
    }
}