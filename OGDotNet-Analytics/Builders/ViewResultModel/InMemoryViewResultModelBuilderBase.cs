//-----------------------------------------------------------------------
// <copyright file="InMemoryViewResultModelBuilderBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders.ViewResultModel
{
    internal abstract class InMemoryViewResultModelBuilderBase<T> : BuilderBase<T>
    {
        protected InMemoryViewResultModelBuilderBase(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override T DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var viewProcIdStr = msg.GetString("viewProcessId");
            UniqueId viewProcessId = UniqueId.Parse(viewProcIdStr);
            var viewCycleIdStr = msg.GetString("viewCycleId");
            UniqueId viewCycleId = UniqueId.Parse(viewCycleIdStr);

            var inputDataTimestamp = msg.GetValue<DateTimeOffset>("valuationTime");
            var resultTimestamp = msg.GetValue<DateTimeOffset>("calculationTime");
            TimeSpan calculationDuration = DurationBuilder.Build(msg.GetMessage("calculationDuration"));
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
                        var map = new Dictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>>();
                        var mapAll = new Dictionary<ComputationTargetSpecification, ISet<ComputedValue>>();

                        foreach (var f in (IFudgeFieldContainer)field.Value)
                        {
                            var v = deserializer.FromField<ComputedValue>(f);

                            ComputationTargetSpecification target = v.Specification.TargetSpecification;

                            if (!map.ContainsKey(target))
                            {
                                map.Add(target, new Dictionary<string, ComputedValue>());
                                mapAll.Add(target, new HashSet<ComputedValue>());
                            }
                            map[target][v.Specification.ValueName] = v; //NOTE: we make an arbitrary choice here
                            mapAll[target].Add(v);
                        }

                        var value = new ViewCalculationResultModel(map, mapAll);

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

        protected abstract T BuildObject(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, Dictionary<string, ViewCalculationResultModel> configurationMap, UniqueId viewProcessId, UniqueId viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, TimeSpan calculationDuration);
    }
}