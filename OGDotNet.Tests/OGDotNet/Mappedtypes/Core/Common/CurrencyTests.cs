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
            Currency.GetInstance("USD");
        }

        [Fact]
        public void CantGetBadCurrenct()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Currency.GetInstance("CurrencyISO::USD"));
        }

        [Fact]
        public void CurrenciesAreEqual()
        {
            var a = Currency.GetInstance("USD");
            var b = Currency.GetInstance("USD");
            Assert.True(a.Equals(b));
            Assert.True(((object) a).Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void CurrenciesAreNotEqual()
        {
            var a = Currency.GetInstance("GBO");
            var b = Currency.GetInstance("USD");
            Assert.False(a.Equals(b));
            Assert.False(((object)a).Equals(b));
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());//This is obviously over strict
        }

        [Fact]
        public void CurrenciesAreSingleton()
        {
            var a = Currency.GetInstance("USD");
            var b = Currency.GetInstance("USD");
            Assert.True(ReferenceEquals(a, b));
        }
    }
}
