//-----------------------------------------------------------------------
// <copyright file="ViewComputationResultModelBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
            UniqueIdentifier viewProcessId = UniqueIdentifier.Parse(msg.GetString("viewProcessId"));
            UniqueIdentifier viewCycleId = UniqueIdentifier.Parse(msg.GetString("viewCycleId"));
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
                        //We need to use the maintain OpenGammaFudgeContext because the optimization is important here
                        // but we can't accidentaly register this twice with our serializer
                        ViewCalculationResultModelBuilder viewCalculationResultModelBuilder = new ViewCalculationResultModelBuilder(deserializer.Context, typeof(ViewCalculationResultModel));
                        var value = viewCalculationResultModelBuilder.DeserializeImpl((FudgeMsg)field.Value, deserializer);
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


            return new InMemoryViewComputationResultModel(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap);
        }
    }
}