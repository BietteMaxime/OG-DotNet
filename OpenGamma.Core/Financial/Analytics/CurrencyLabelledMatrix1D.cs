// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrencyLabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Util.Money;

namespace OpenGamma.Financial.Analytics
{
    [FudgeSurrogate(typeof(CurrencyLabelledMatrix1DBuilder))]
    public class CurrencyLabelledMatrix1D : LabelledMatrix1D<Currency>
    {
        public CurrencyLabelledMatrix1D(IList<Currency> keys, IList<object> labels, IList<double> values, string labelsTitle = null, string valuesTitle = null)
            : base(keys, labels, values, labelsTitle, valuesTitle)
        {
        }
    }
}
