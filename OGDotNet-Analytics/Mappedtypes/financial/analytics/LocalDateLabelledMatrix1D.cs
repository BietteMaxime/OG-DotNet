//-----------------------------------------------------------------------
// <copyright file="LocalDateLabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Financial.Analytics
{
    [FudgeSurrogate(typeof(LabelledMatrix1DBuilder<FudgeDate, LocalDateLabelledMatrix1D>))]
    public class LocalDateLabelledMatrix1D : LabelledMatrix1D<FudgeDate>
    {
        public LocalDateLabelledMatrix1D(IList<FudgeDate> keys, IList<object> labels, IList<double> values) : base(keys, labels, values)
        {
        }
    }
}
