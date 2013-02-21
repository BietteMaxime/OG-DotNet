// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleLabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Financial.Analytics
{
    [FudgeSurrogate(typeof(LabelledMatrix1DBuilder<double, DoubleLabelledMatrix1D>))]
    public class DoubleLabelledMatrix1D : LabelledMatrix1D<double>
    {
        public DoubleLabelledMatrix1D(IList<double> keys, IList<object> labels, IList<double> values, string labelsTitle = null, string valuesTitle = null) : base(keys, labels, values, labelsTitle, valuesTitle)
        {
        }
    }
}