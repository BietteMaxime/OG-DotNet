//-----------------------------------------------------------------------
// <copyright file="DoubleLabelledMatrix2DBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.Financial.Analytics;

namespace OGDotNet.Builders
{
    class DoubleLabelledMatrix2DBuilder : BuilderBase<DoubleLabelledMatrix2D>
    {
        private const string MatrixField = "matrix";
        private const int XLabelTypeOrdinal = 0;
        private const int XKeyOrdinal = 1;
        private const int XLabelOrdinal = 2;
        private const int YLabelTypeOrdinal = 3;
        private const int YKeyOrdinal = 4;
        private const int YLabelOrdinal = 5;
        private const int ValueOrdinal = 6;

        public DoubleLabelledMatrix2DBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        public override DoubleLabelledMatrix2D DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            IFudgeFieldContainer msg = ffc.GetMessage(MatrixField);

            var xLabelTypes = new Queue<string>();
            var xLabelValues = new Queue<IFudgeField>();
            var yLabelTypes = new Queue<string>();
            var yLabelValues = new Queue<IFudgeField>();

            var xKeys = new List<double>();
            var xLabels = new List<object>();
            var yKeys = new List<double>();
            var yLabels = new List<object>();
            var values = new List<List<double>>();

            bool newRow = true;
            int count = -1;
            foreach (var field in msg)
            {
                switch (field.Ordinal)
                {
                    case XLabelTypeOrdinal:
                        xLabelTypes.Enqueue((string)field.Value);
                        break;
                    case XKeyOrdinal:
                        xKeys.Add((double)field.Value);
                        break;
                    case XLabelOrdinal:
                        xLabelValues.Enqueue(field);
                        break;
                    case YLabelTypeOrdinal:
                        newRow = true;
                        count++;
                        yLabelTypes.Enqueue((string)field.Value);
                        break;
                    case YKeyOrdinal:
                        yKeys.Add((double)field.Value);
                        break;
                    case YLabelOrdinal:
                        yLabelValues.Enqueue(field);
                        break;
                    case ValueOrdinal:
                        var value = (double)field.Value;
                        if (newRow)
                        {
                            var row = new List<double> { value };
                            values.Add(row);
                            newRow = false;
                        }
                        else
                        {
                            values[count].Add(value);
                        }
                        break;
                }

                var typeMap = (IFudgeTypeMappingStrategy)deserializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
                if (xLabelTypes.Any() && xLabelValues.Any())
                {
                    // Have a type and a value, which can be consumed
                    string labelType = xLabelTypes.Dequeue();
                    IFudgeField labelValue = xLabelValues.Dequeue();

                    object label = Deserialize(deserializer, typeMap, labelType, labelValue);
                    xLabels.Add(label);
                }
                if (yLabelTypes.Any() && yLabelValues.Any())
                {
                    // Have a type and a value, which can be consumed
                    string labelType = yLabelTypes.Dequeue();
                    IFudgeField labelValue = yLabelValues.Dequeue();

                    object label = Deserialize(deserializer, typeMap, labelType, labelValue);
                    yLabels.Add(label);
                }
            }
            int matrixRowSize = yKeys.Count;
            int matrixColumnSize = xKeys.Count;
            var xKeysArray = new double[matrixColumnSize];
            var xLabelsArray = new object[matrixColumnSize];
            var yKeysArray = new double[matrixRowSize];
            var yLabelsArray = new object[matrixRowSize];

            var valuesArray = new double[matrixRowSize][];

            for (int i = 0; i < matrixRowSize; i++)
            {
                yKeysArray[i] = yKeys[i];
                yLabelsArray[i] = yLabels[i];
                for (int j = 0; j < matrixColumnSize; j++)
                {
                    if (i == 0)
                    {
                        xKeysArray[j] = xKeys[j];
                        xLabelsArray[j] = xLabels[j];
                    }
                    if (valuesArray[i] == null)
                    {
                        valuesArray[i] = new double[matrixColumnSize];
                    }
                    valuesArray[i][j] = values[i][j];
                }
            }
            return new DoubleLabelledMatrix2D(xKeysArray, yKeysArray, xLabelsArray, yLabelsArray, valuesArray);
        }

        /// <remarks>
        ///     See also <see cref="ComputedValueBuilder.GetValue"/>
        /// </remarks>
        private static object Deserialize(IFudgeDeserializer deserializer, IFudgeTypeMappingStrategy typeMap, string labelType, IFudgeField labelValue)
        {
            if (labelValue.Type != FudgeMsgFieldType.Instance)
            {
                return labelValue.Value;
            }
            Type labelClass = typeMap.GetType(labelType);
            return deserializer.FromField(labelValue, labelClass);
        }
    }
}
