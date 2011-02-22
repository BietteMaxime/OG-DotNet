using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteCurrencyMatrixSourceTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            Assert.NotNull(Context.CurrencyMatrixSource);
        }

        [Fact]
        public void CanGetNonExistantMatrix()
        {
            var source = Context.CurrencyMatrixSource;
            Assert.Null(source.GetCurrencyMatrix("NonExistant"+Guid.NewGuid()));
        }

        [Fact]
        public void CanGetBloombergMatrix()
        {
            var source = Context.CurrencyMatrixSource;
            var currencyMatrix = source.GetCurrencyMatrix("BloombergLiveData");
            Assert.NotNull(currencyMatrix);

            Assert.True(currencyMatrix.SourceCurrencies.Contains(Currency.Create("USD")));
            Assert.True(currencyMatrix.SourceCurrencies.Contains(Currency.Create("GBP")));
            Assert.True(currencyMatrix.TargetCurrencies.Contains(Currency.Create("USD")));
            Assert.True(currencyMatrix.TargetCurrencies.Contains(Currency.Create("GBP")));

            var valueTypes = new HashSet<Type>();
            var matchedTargets = new HashSet<Currency>();
            foreach (var sourceCurrency in currencyMatrix.SourceCurrencies)
            {
                var foundTargets = currencyMatrix.TargetCurrencies.Where(targetCurrency => currencyMatrix.GetConversion(sourceCurrency, targetCurrency) != null);
                Assert.NotEmpty(foundTargets);
                foreach (var foundTarget in foundTargets)
                {
                    matchedTargets.Add(foundTarget);
                    valueTypes.Add(currencyMatrix.GetConversion(sourceCurrency, foundTarget).GetType());
                }
            }

            foreach (var targetCurrency in currencyMatrix.TargetCurrencies)
            {
                Assert.Contains(targetCurrency,matchedTargets);
            }
        }
    }
}
