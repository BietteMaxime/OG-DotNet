//-----------------------------------------------------------------------
// <copyright file="CurrencyLabelledMatrix1DBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.Util.Money;

namespace OGDotNet.Builders
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