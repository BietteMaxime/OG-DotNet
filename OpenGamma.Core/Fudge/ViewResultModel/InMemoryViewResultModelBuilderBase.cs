// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryViewResultModelBuilderBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Engine;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Id;

namespace OpenGamma.Fudge.ViewResultModel
{
    internal abstract class InMemoryViewResultModelBuilderBase<T> : BuilderBase<T>
    {
        protected InMemoryViewResultModelBuilderBase(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override T DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            UniqueId viewProcessId = GetUniqueId(msg, "viewProcessId");
            UniqueId viewCycleId = GetUniqueId(msg, "viewCycleId");

            var inputDataTimestamp = GetToDateTimeOffsetWithDefault(msg, "valuationTime"); 
            var resultTimestamp = GetToDateTimeOffsetWithDefault(msg, "calculationTime");
            TimeSpan calculationDuration = DurationBuilder.Build(msg.GetMessage("calculationDuration")).GetValueOrDefault(); // TODO strict once PLAT-1683
            var configurationMap = new Dictionary<string, ViewCalculationResultModel>();
            var keys = new Queue<string>();
            var values = new Queue<ViewCalculationResultModel>();

            foreach (var field in msg.GetMessage("results"))
            {
                switch (field.Ordinal)
                {
                    case 1:
                        string key = field.GetString();
                        if (!values.Any())
                        {
                            keys.Enqueue(key);
                        }
                        else
                        {
                            configurationMap.Add(key, values.Dequeue());
                        }

                        break;
                    case 2:
                        var mapAll = new Dictionary<ComputationTargetSpecification, ISet<ComputedValue>>();

                        foreach (var f in (IFudgeFieldContainer)field.Value)
                        {
                            var v = deserializer.FromField<ComputedValue>(f);

                            ComputationTargetSpecification target = v.Specification.TargetSpecification;

                            if (!mapAll.ContainsKey(target))
                            {
                                mapAll.Add(target, new HashSet<ComputedValue>());
                            }

                            mapAll[target].Add(v);
                        }

                        var value = new ViewCalculationResultModel(mapAll);

                        if (!keys.Any())
                        {
                            values.Enqueue(value);
                        }
                        else
                        {
                            configurationMap.Add(keys.Dequeue(), value);
                        }

                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            return BuildObject(msg, deserializer, configurationMap, viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, calculationDuration);
        }

        private static DateTimeOffset GetToDateTimeOffsetWithDefault(IFudgeFieldContainer msg, string fieldName)
        {
            // TODO strict once [PLAT-1683] is fixed
            if (msg.GetByName(fieldName).Type == IndicatorFieldType.Instance)
            {
                return new DateTimeOffset();
            }

            return msg.GetValue<DateTimeOffset>(fieldName);
        }

        private static UniqueId GetUniqueId(IFudgeFieldContainer msg, string fieldName)
        {
            // TODO: remove this once PLAT-1683 is fixed
            var uidStr = msg.GetString(fieldName);
            return uidStr == string.Empty ? null : UniqueId.Parse(uidStr);
        }

        protected abstract T BuildObject(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, Dictionary<string, ViewCalculationResultModel> configurationMap, UniqueId viewProcessId, UniqueId viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, TimeSpan calculationDuration);
    }
}