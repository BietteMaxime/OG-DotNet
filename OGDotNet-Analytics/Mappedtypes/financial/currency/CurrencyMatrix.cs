using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;


namespace OGDotNet.Mappedtypes.financial.currency
{
    [FudgeSurrogate(typeof(CurrencyMatrixBuilder))]
    public abstract class CurrencyMatrix
    {
        public abstract ICollection<Currency> SourceCurrencies { get; }

        public abstract IEnumerable<Currency> TargetCurrencies { get; }

        public abstract CurrencyMatrixValue GetConversion(Currency source, Currency target);    
    }
} 