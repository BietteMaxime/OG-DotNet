using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet_Analytics.Mappedtypes.engine;
using OGDotNet_Analytics.Mappedtypes.engine.View;
using OGDotNet_Analytics.Model;

namespace OGDotNet_Analytics.Builders
{
    internal class ViewComputationResultModelBuilder : BuilderBase<ViewComputationResultModel>
    {
        public ViewComputationResultModelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ViewComputationResultModel DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var viewName = msg.GetValue<String>("viewName");
            var inputDataTimestamp = msg.GetValue<FudgeDateTime>("valuationTS");
            var resultTimestamp = msg.GetValue<FudgeDateTime>("resultTS");
            var configurationMap = new Dictionary<String, ViewCalculationResultModel>();
            var keys = new Queue<String>();
            var values = new Queue<ViewCalculationResultModel>();

            foreach (var field in (IFudgeFieldContainer) msg.GetByName("results").Value)
            {
                switch (field.Ordinal)
                {
                    case 1:
                        String key = field.GetString();
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
                        var value = FudgeConfig.GetFudgeSerializer().Deserialize<ViewCalculationResultModel>((FudgeMsg) field.Value);
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

            IDictionary<ComputationTargetSpecification, ViewTargetResultModelImpl> targetMap = new Dictionary<ComputationTargetSpecification, ViewTargetResultModelImpl>();
            foreach (var configurationEntry in configurationMap)
            {
                foreach (ComputationTargetSpecification targetSpec in configurationEntry.Value.getAllTargets())
                {

                    ViewTargetResultModelImpl targetResult;
                    if (! targetMap.TryGetValue(targetSpec, out targetResult))
                    {
                        targetResult = new ViewTargetResultModelImpl();
                        targetMap.Add(targetSpec, targetResult);
                    }

                    targetResult.AddAll(configurationEntry.Key, configurationEntry.Value[targetSpec]);
                }
            }
    
            var allResults = new List<ViewResultEntry>();
            foreach (var configurationEntry in configurationMap)
            {
                foreach (var targetSpec in configurationEntry.Value.getAllTargets())
                {
                    var results = configurationEntry.Value[targetSpec];
                    foreach (var value in results)
                    {
                        allResults.Add(new ViewResultEntry(configurationEntry.Key, value.Value));
                    }
                }
            }
            
            return new ViewComputationResultModel(viewName, inputDataTimestamp, resultTimestamp, configurationMap, targetMap.ToDictionary(kvp => kvp.Key, kvp => (IViewTargetResultModel) kvp.Value), allResults);
        }
    }
}