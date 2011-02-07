using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics
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

        private const string MATRIX_FIELD = "matrix";
        private const int LABEL_TYPE_ORDINAL = 0;
        private const int KEY_ORDINAL = 1;
        private const int LABEL_ORDINAL = 2;
        private const int VALUE_ORDINAL = 3;

        public static DoubleLabelledMatrix1D FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var msg = ffc.GetMessage(MATRIX_FIELD);

            var labelTypes = new Queue<string>();
            var labelValues = new Queue<IFudgeField>();

            IList<double> keys = new List<double>();
            IList<object> labels = new List<object>();
            IList<double> values = new List<double>();

            foreach (IFudgeField field in msg)
            {
                switch (field.Ordinal)
                {
                    case LABEL_TYPE_ORDINAL:
                        labelTypes.Enqueue((string)field.Value);
                        break;
                    case KEY_ORDINAL:
                        keys.Add((double)field.Value);
                        break;
                    case LABEL_ORDINAL:
                        labelValues.Enqueue(field);
                        break;
                    case VALUE_ORDINAL:
                        values.Add((double)field.Value);
                        break;
                }

                if (labelTypes.Count != 0 && labelValues.Count != 0)
                {
                    // Have a type and a value, which can be consumed
                    string labelTypeName = labelTypes.Dequeue();
                    var typeMapper = (IFudgeTypeMappingStrategy)deserializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
                    Type labelType = typeMapper.GetType(labelTypeName);

                    IFudgeField labelValue = labelValues.Dequeue();

                    if (labelType == typeof(Tenor))
                    {
                        //TODO hack hack hack, this seems to get serialized as a string :S
                        string period = (string)labelValue.Value;
                        labels.Add(new Tenor(period));
                    }
                    else
                    {//TODO work out whether this is right in the general case
                        object label = deserializer.FromField(labelValue, labelType);
                        labels.Add(label);
                    }
                }
            }

            return new DoubleLabelledMatrix1D(keys, labels, values);
        }


        /// <summary>
        /// Array here are packed YAN way
        /// </summary>
        private static List<T> GetArray<T>(IFudgeFieldContainer ffc, string fieldName)
        {
            var fudgeFields = ffc.GetMessage(fieldName).GetAllFields();

            return fudgeFields.Select(fudgeField => (T)fudgeField.Value).ToList();
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