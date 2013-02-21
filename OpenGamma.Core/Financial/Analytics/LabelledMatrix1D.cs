// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabelledMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using OpenGamma.Util;

namespace OpenGamma.Financial.Analytics
{
    public class LabelledMatrix1D<TKey> : IEnumerable<LabelledMatrixEntry>
    {
        private readonly IList<TKey> _keys;
        private readonly IList<object> _labels;
        private readonly IList<double> _values;

        private readonly string _labelsTitle;
        private readonly string _valuesTitle;

        public LabelledMatrix1D(IList<TKey> keys, IList<object> labels, IList<double> values, string labelsTitle = null, string valuesTitle = null)
        {
            ArgumentChecker.NotNull(keys, "keys");
            ArgumentChecker.NotNull(labels, "labels");
            ArgumentChecker.NotNull(values, "values");
            ArgumentChecker.Not(keys.Count != labels.Count, "Labelled matrix is the wrong shape");
            ArgumentChecker.Not( keys.Count != values.Count, "Labelled matrix is the wrong shape");
            
            _keys = keys;
            _valuesTitle = valuesTitle;
            _labelsTitle = labelsTitle;
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

        public string LabelsTitle
        {
            get { return _labelsTitle; }
        }

        public string ValuesTitle
        {
            get { return _valuesTitle; }
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