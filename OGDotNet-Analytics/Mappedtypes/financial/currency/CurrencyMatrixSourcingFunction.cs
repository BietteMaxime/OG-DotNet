//-----------------------------------------------------------------------
// <copyright file="CurrencyMatrixSourcingFunction.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.engine.value;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.financial.currency
{
    public class CurrencyMatrixSourcingFunction
    {
        private readonly CurrencyMatrix _matrix;

        public CurrencyMatrixSourcingFunction(CurrencyMatrix matrix)
        {
            _matrix = matrix;
        }

        public double GetConversionRate(Func<ValueRequirement, double> inputs, Currency source, Currency target)
        {
            var currencyMatrixValue = _matrix.GetConversion(source, target);
            if (currencyMatrixValue is CurrencyMatrixValue.CurrencyMatrixCross)
            {
                var cross = (CurrencyMatrixValue.CurrencyMatrixCross)currencyMatrixValue;
                return GetConversionRate(inputs, source, cross.CrossCurrency) * GetConversionRate(inputs, cross.CrossCurrency, target);
            }
            else if (currencyMatrixValue is CurrencyMatrixValue.CurrencyMatrixFixed)
            {
                var fixedValue = (CurrencyMatrixValue.CurrencyMatrixFixed)currencyMatrixValue;
                return fixedValue.FixedValue;
            }
            else if (currencyMatrixValue is CurrencyMatrixValue.CurrencyMatrixValueRequirement)
            {
                var valueRequirement = (CurrencyMatrixValue.CurrencyMatrixValueRequirement)currencyMatrixValue;

                double rate = inputs(valueRequirement.ValueRequirement);
                if (valueRequirement.Reciprocal)
                {
                    rate = 1.0 / rate;
                }
                return rate;
            }
            else if (currencyMatrixValue == null)
            {
                throw new ArgumentException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
