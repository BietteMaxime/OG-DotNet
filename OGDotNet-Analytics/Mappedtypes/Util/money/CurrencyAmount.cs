//-----------------------------------------------------------------------
// <copyright file="CurrencyAmount.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Core.Common;

namespace OGDotNet.Mappedtypes.Util.money
{
    public class CurrencyAmount
    {
        private readonly Currency _currency;
        private readonly double _amount;

        public CurrencyAmount(Currency currency, double amount)
        {
            _currency = currency;
            _amount = amount;
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public double Amount
        {
            get { return _amount; }
        }
    }
}