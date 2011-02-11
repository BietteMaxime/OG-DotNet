using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Mappedtypes.financial.analytics
{
    public class LabelledMatrix1D<TKey> : IEnumerable<LabelledMatrixEntry>
    {
        private readonly IList<TKey> _keys;
        private readonly IList<object> _labels;
        private readonly IList<double> _values;

        protected LabelledMatrix1D(IList<TKey> keys, IList<object> labels, IList<double> values)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            if (labels == null) throw new ArgumentNullException("labels");
            if (values == null) throw new ArgumentNullException("values");
            if (keys.Count != labels.Count || keys.Count != values.Count)
            {
                throw new ArgumentException("Labelled matrix is the wrong shape");
            }
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