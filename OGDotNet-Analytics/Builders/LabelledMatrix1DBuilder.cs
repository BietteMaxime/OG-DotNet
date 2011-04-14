//-----------------------------------------------------------------------
// <copyright file="LabelledMatrix1DBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Builders
{
    /// <summary>
    /// I would like a better way of accesing this than the <see cref="FudgeSurrogateAttribute"/>
    /// I would also like to check that things type check nicely.
    /// And a pony
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TMatrix"></typeparam>
    internal class LabelledMatrix1DBuilder<TKey, TMatrix> : BuilderBase<TMatrix> where TMatrix : LabelledMatrix1D<TKey>
    {
        private const string MatrixField = "matrix";
        private const int LabelTypeOrdinal = 0;
        private const int KeyOrdinal = 1;
        private const int LabelOrdinal = 2;
        private const int ValueOrdinal = 3;

        public LabelledMatrix1DBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override TMatrix DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var msg = ffc.GetMessage(MatrixField);

            var labelTypes = new Queue<string>();
            var labelValues = new Queue<IFudgeField>();

            IList<TKey> keys = new List<TKey>();
            IList<object> labels = new List<object>();
            IList<double> values = new List<double>();

            foreach (IFudgeField field in msg)
            {
                switch (field.Ordinal)
                {
                    case LabelTypeOrdinal:
                        labelTypes.Enqueue((string)field.Value);
                        break;
                    case KeyOrdinal:
                        keys.Add((TKey)field.Value);
                        break;
                    case LabelOrdinal:
                        labelValues.Enqueue(field);
                        break;
                    case ValueOrdinal:
                        values.Add((double)field.Value);
                        break;
                }

                if (labelTypes.Count != 0 && labelValues.Count != 0)
                {
                    // Have a type and a value, which can be consumed
                    string labelTypeName = labelTypes.Dequeue();
                    IFudgeField labelValue = labelValues.Dequeue();

                    if (labelTypeName == "java.lang.String")
                    {
                        var value = (string)labelValue.Value;
                        labels.Add(value);
                    }
                    else if (labelTypeName == "com.opengamma.util.time.Tenor")
                    {
                        //TODO DOTNET-14 this is serialized as a string here
                        string period = (string)labelValue.Value;
                        labels.Add(new Tenor(period));
                    }
                    else
                    {//TODO work out whether this is right (and fast enough) in the general case
                        var typeMapper = (IFudgeTypeMappingStrategy)Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
                        Type labelType = typeMapper.GetType(labelTypeName);

                        object label = deserializer.FromField(labelValue, labelType);
                        labels.Add(label);
                    }
                }
            }

            var constructorInfo = typeof(TMatrix).GetConstructor(new[]
                                                                     {
                                                                         typeof(IList<TKey>),
                                                                         typeof(IList<object>), //TODO type up this (if the java side does)
                                                                         typeof(IList<double>)
                                                                     });
            return (TMatrix)constructorInfo.Invoke(new object[] { keys, labels, values });
        }
    }
}