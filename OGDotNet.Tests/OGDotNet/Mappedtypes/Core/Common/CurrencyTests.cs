using System;
using Xunit;
using OGDotNet.Mappedtypes.Core.Common;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Core.Common
{
    public class CurrencyTests
    {
        [Fact]
        public void CanCreateCurrency()
        {
            Currency.Create("USD");
        }

        [Fact]
        public void CantGetBadCurrenct()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Currency.Create("CurrencyISO::USD"));
        }

        [Fact]
        public void CurrenciesAreEqual()
        {
            var a = Currency.Create("USD");
            var b = Currency.Create("USD");
            Assert.True(a.Equals(b));
            Assert.True(((object) a).Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void CurrenciesAreNotEqual()
        {
            var a = Currency.Create("GBO");
            var b = Currency.Create("USD");
            Assert.False(a.Equals(b));
            Assert.False(((object)a).Equals(b));
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());//This is obviously over strict
        }

        [Fact]
        public void CurrenciesAreSingleton()
        {
            var a = Currency.Create("USD");
            var b = Currency.Create("USD");
            Assert.True(ReferenceEquals(a, b));
        }
    }
}
