using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.financial.analytics
{
    [FudgeSurrogate(typeof(LabelledMatrix1DBuilder<double, DoubleLabelledMatrix1D>))]
    public class DoubleLabelledMatrix1D : LabelledMatrix1D<double>
    {
        public DoubleLabelledMatrix1D(IList<double> keys, IList<object> labels, IList<double> values) : base(keys, labels, values)
        {
        }
    }
}