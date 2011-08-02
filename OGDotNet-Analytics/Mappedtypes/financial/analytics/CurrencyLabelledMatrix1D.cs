//-----------------------------------------------------------------------
// <copyright file="CurrencyLabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Util.Money;

namespace OGDotNet.Mappedtypes.Financial.Analytics
{
    [FudgeSurrogate(typeof(CurrencyLabelledMatrix1DBuilder))]
    public class CurrencyLabelledMatrix1D : LabelledMatrix1D<Currency>
    {
        public CurrencyLabelledMatrix1D(IList<Currency> keys, IList<object> labels, IList<double> values)
            : base(keys, labels, values)
        {
        }
    }
}
