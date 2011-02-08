using System.Collections.Generic;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.financial.analytics
{
    [FudgeSurrogate(typeof(LabelledMatrix1DBuilder<FudgeDate, LocalDateLabelledMatrix1D>))]
    public class LocalDateLabelledMatrix1D : LabelledMatrix1D<FudgeDate>
    {
        public LocalDateLabelledMatrix1D(IList<FudgeDate> keys, IList<object> labels, IList<double> values) : base(keys, labels, values)
        {
        }
    }
}
