using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Mappedtypes.financial.analytics
{
    public class DoubleLabelledMatrix1D : IEnumerable<LabelledMatrixEntry>
    {
        private readonly IList<double> _keys;
        private readonly IList<object> _labels;
        private readonly IList<double> _values;

        private DoubleLabelledMatrix1D(IList<double> keys, IList<object> labels, IList<double> values)
        {
            _keys = keys;
            _labels = labels;
            _values = values;
        }

        public IList<double> Keys
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

        public static DoubleLabelledMatrix1D FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var keys = GetArray<double>(ffc, "keys");//This is a java Double[] go go go poor generics
            var values = ffc.GetValue<double[]>("values");//This is a java (and hence a fudge) double[]
            var labels = GetArray<object>(ffc, "labels");


            return new DoubleLabelledMatrix1D(keys, labels, values);
        }

        /// <summary>
        /// Array here are packed YAN way
        /// </summary>
        private static List<T> GetArray<T>(IFudgeFieldContainer ffc, string fieldName)
        {
            var fudgeFields = ffc.GetMessage(fieldName).GetAllFields();

            return fudgeFields.Select(fudgeField => (T) fudgeField.Value).ToList();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
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
            return _labels.Zip(_keys, (l, k) => new LabelledMatrixEntry(l, k));
        }
    }
}