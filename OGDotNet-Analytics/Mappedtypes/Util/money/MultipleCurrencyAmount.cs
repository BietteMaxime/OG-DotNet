//-----------------------------------------------------------------------
// <copyright file="MultipleCurrencyAmount.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Util.Money
{
    public class MultipleCurrencyAmount : IEnumerable<CurrencyAmount>
    {
        private readonly SortedDictionary<Currency, CurrencyAmount> _amounts;

        public MultipleCurrencyAmount(SortedDictionary<Currency, CurrencyAmount> amounts)
        {
            _amounts = amounts;
        }

        public SortedDictionary<Currency, CurrencyAmount> Amounts
        {
            get { return _amounts; }
        }

        public IEnumerator<CurrencyAmount> GetEnumerator()
        {
            return _amounts.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static MultipleCurrencyAmount FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var currencies = ffc.GetMessage("currencies").Select(f => (string)f.Value).Select(Currency.Create).ToList();
            var amounts = (double[])ffc.GetByName("amounts").Value;
            if (currencies.Count != amounts.Length)
            {
                throw new ArgumentException("Mismatched labels and value", "ffc");
            }
            var dict = new SortedDictionary<Currency, CurrencyAmount>();
            for (int i = 0; i < currencies.Count; i++)
            {
                dict.Add(currencies[i], new CurrencyAmount(currencies[i], amounts[i]));
            }
            return new MultipleCurrencyAmount(dict);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
