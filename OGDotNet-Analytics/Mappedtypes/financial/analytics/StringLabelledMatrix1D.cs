//-----------------------------------------------------------------------
// <copyright file="StringLabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Financial.Analytics
{
    [FudgeSurrogate(typeof(StringLabelledMatrix1DBuilder))]
    public class StringLabelledMatrix1D : LabelledMatrix1D<string>
    {
        public StringLabelledMatrix1D(IList<string> keys, IList<object> labels, IList<double> values, string labelsTitle = null, string valuesTitle = null)
            : base(keys, keys.Cast<object>().ToList(), values, labelsTitle, valuesTitle)
        {
            ArgumentChecker.Not(labels.Any(), "Didn't expect separate labels");
        }
    }
}