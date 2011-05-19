//-----------------------------------------------------------------------
// <copyright file="ViewComputationResultModelBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    internal class ViewComputationResultModelBuilder : BuilderBase<InMemoryViewComputationResultModel>
    {
        public ViewComputationResultModelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override InMemoryViewComputationResultModel DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            //TODO: these are supposed to be reliably non null
            var viewProcIdStr = msg.GetString("viewProcessId");
            UniqueIdentifier viewProcessId = string.IsNullOrEmpty(viewProcIdStr) ? null : UniqueIdentifier.Parse(viewProcIdStr);
            var viewCycleIdStr = msg.GetString("viewCycleId");
            UniqueIdentifier viewCycleId = string.IsNullOrEmpty(viewCycleIdStr) ? null : UniqueIdentifier.Parse(msg.GetString("viewCycleId"));

            var inputDataTimestamp = msg.GetValue<DateTimeOffset>("valuationTS");
            var resultTimestamp = msg.GetValue<DateTimeOffset>("resultTS");
            var configurationMap = new Dictionary<string, ViewCalculationResultModel>();
            var keys = new Queue<string>();
            var values = new Queue<ViewCalculationResultModel>();

            foreach (var field in (IFudgeFieldContainer) msg.GetByName("results").Value)
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

                        foreach (var f in (IFudgeFieldContainer) field.Value)
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

            var liveDataMsg = msg.GetMessage("liveData");
            List<ComputedValue> liveData = liveDataMsg == null ? null : liveDataMsg.GetAllByOrdinal(1).Select(deserializer.FromField<ComputedValue>).ToList();
            return new InMemoryViewComputationResultModel(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap, liveData);
        }
    }
}