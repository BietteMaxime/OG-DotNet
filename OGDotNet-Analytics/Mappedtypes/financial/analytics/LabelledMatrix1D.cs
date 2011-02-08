using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Mappedtypes.financial.analytics
{
    /// <summary>
    /// TODO variance, and match mapped types
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class LabelledMatrix1D<TKey> : IEnumerable<LabelledMatrixEntry>
    {
        private readonly IList<TKey> _keys;
        private readonly IList<object> _labels;
        private readonly IList<double> _values;

        protected LabelledMatrix1D(IList<TKey> keys, IList<object> labels, IList<double> values)
        {
            _keys = keys;
            _labels = labels;
            _values = values;
        }

        public IList<TKey> Keys
        {
            get { return _keys; }
        }

        public IList<object> Labels
        {
            get { return _labels; }
        }

        public IList<double> Values
        {
            get { return _values; }
        }

        public IEnumerator<LabelledMatrixEntry> GetEnumerator()
        {
            return GetEntries().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<LabelledMatrixEntry> GetEntries()
        {
            return _labels.Zip(_values, (l, k) => new LabelledMatrixEntry(l, k));
        }
    }
}