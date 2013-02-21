// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICurrencyMatrix.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Id;
using OpenGamma.Util.Money;

namespace OpenGamma.Financial.Ccy
{
    public interface ICurrencyMatrix : IUniqueIdentifiable
    {
        ICollection<Currency> SourceCurrencies { get; }

        IEnumerable<Currency> TargetCurrencies { get; }

        CurrencyMatrixValue GetConversion(Currency source, Currency target);    
    }
} 