// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrencyLabelledMatrix1DBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;

using OpenGamma.Financial.Analytics;
using OpenGamma.Util.Money;

namespace OpenGamma.Fudge
{
    internal class CurrencyLabelledMatrix1DBuilder : LabelledMatrix1DBuilder<Currency, CurrencyLabelledMatrix1D> 
    {
        public CurrencyLabelledMatrix1DBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override Currency GetKey(IFudgeField field)
        {
            return Currency.Create((string) field.Value);
        }
    }
}