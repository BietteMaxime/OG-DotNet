//-----------------------------------------------------------------------
// <copyright file="StringLabelledMatrix1DBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using OGDotNet.Mappedtypes.Financial.Analytics;

namespace OGDotNet.Builders
{
    internal class StringLabelledMatrix1DBuilder : LabelledMatrix1DBuilder<string, StringLabelledMatrix1D>
    {
        public StringLabelledMatrix1DBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        protected override string GetKey(IFudgeField field)
        {
            return (string)field.Value;
        }
    }
}