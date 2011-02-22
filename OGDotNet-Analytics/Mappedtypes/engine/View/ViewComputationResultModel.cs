using System;
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewComputationResultModelBuilder))]
    public class ViewComputationResultModel
    {
        private readonly string _viewName;
        private readonly FudgeDateTime _inputDataTimestamp;
        private readonly FudgeDateTime _resultTimestamp;
        private readonly IDictionary<string, ViewCalculationResultModel> _configurationMap;

        public ViewComputationResultModel(string viewName, FudgeDateTime inputDataTimestamp, FudgeDateTime resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap)
        {
            _viewName = viewName;
            _inputDataTimestamp = inputDataTimestamp;
            _resultTimestamp = resultTimestamp;
            _configurationMap = configurationMap;
        }


        public FudgeDateTime ValuationTime { get { return _inputDataTimestamp; } }
        public FudgeDateTime ResultTimestamp { get { return _resultTimestamp; } }
        public String ViewName { get { return _viewName; } }

        public bool TryGetValue(string calculationConfiguration, ValueRequirement valueRequirement, out object result)
        {            
            result = null;
            
            ViewCalculationResultModel model;
            if (!_configurationMap.TryGetValue(calculationConfiguration, out model))
            {
                return false;
            }
            ComputedValue computedValue;
            if (!model.TryGetValue(valueRequirement.TargetSpecification, valueRequirement.ValueName, out computedValue))
            {
                return false;
            }
            result = computedValue.Value;
            return true;
        }

        public IEnumerable<ViewResultEntry> AllResults
        {
            get { return _configurationMap.SelectMany(config => config.Value.AllResults.Select(cv => new ViewResultEntry(config.Key, cv))); }
        }


        public ViewComputationResultModel ApplyDelta(ViewComputationResultModel delta)
        {
            if (!string.IsNullOrEmpty(delta._viewName) && delta._viewName != _viewName)
                throw new ArgumentException("View name changed unexpectedly");

            var viewCalculationResultModels = _configurationMap.Merge(delta._configurationMap, (a,b) =>a.ApplyDelta(b));

            return new ViewComputationResultModel(
                _viewName, 
                delta._inputDataTimestamp, 
                delta._resultTimestamp, 
                viewCalculationResultModels
                );
        }
    }
}