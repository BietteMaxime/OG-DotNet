//-----------------------------------------------------------------------
// <copyright file="InMemoryViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class InMemoryViewComputationResultModel : ViewComputationResultModel
    {
        private readonly string _viewName;
        private readonly DateTimeOffset _inputDataTimestamp;
        private readonly DateTimeOffset _resultTimestamp;
        private readonly IDictionary<string, ViewCalculationResultModel> _configurationMap;

        public InMemoryViewComputationResultModel(string viewName, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap)
        {
            _viewName = viewName;
            _inputDataTimestamp = inputDataTimestamp;
            _resultTimestamp = resultTimestamp;
            _configurationMap = configurationMap;
        }

        public DateTimeOffset ValuationTime { get { return _inputDataTimestamp; } }
        public DateTimeOffset ResultTimestamp { get { return _resultTimestamp; } }
        public string ViewName { get { return _viewName; } }

        public ComputedValue this[string calculationConfiguration, ValueRequirement valueRequirement]
        {
            get
            {
                ComputedValue ret;
                if (!TryGetComputedValue(calculationConfiguration, valueRequirement, out ret))
                {
                    throw new KeyNotFoundException();
                }
                return ret;
            }
        }

        public bool TryGetValue(string calculationConfiguration, ValueRequirement valueRequirement, out object result)
        {
            ComputedValue compValue;
            if (!TryGetComputedValue(calculationConfiguration, valueRequirement, out compValue))
            {
                result = null;
                return false;
            }
            result = compValue.Value;
            return true;
        }

        public bool TryGetComputedValue(string calculationConfiguration, ValueRequirement valueRequirement, out ComputedValue result)
        {
            result = null;

            ViewCalculationResultModel model;
            if (!_configurationMap.TryGetValue(calculationConfiguration, out model))
            {
                return false;
            }

            if (!model.TryGetValue(valueRequirement.TargetSpecification, valueRequirement.ValueName, out result))
            {
                return false;
            }
            return true;
        }

        public IEnumerable<ViewResultEntry> AllResults
        {
            get { return _configurationMap.SelectMany(config => config.Value.AllResults.Select(cv => new ViewResultEntry(config.Key, cv))); }
        }

        public IDictionary<string, ViewCalculationResultModel> CalculationResultsByConfiguration
        {
            get { return _configurationMap; }
        }

        public InMemoryViewComputationResultModel ApplyDelta(InMemoryViewComputationResultModel delta)
        {
            if (!string.IsNullOrEmpty(delta._viewName) && delta._viewName != _viewName)
                throw new ArgumentException("View name changed unexpectedly");

            var viewCalculationResultModels = _configurationMap.Merge(delta._configurationMap, (a, b) => a.ApplyDelta(b));

            return new InMemoryViewComputationResultModel(
                _viewName,
                delta._inputDataTimestamp,
                delta._resultTimestamp,
                viewCalculationResultModels
                );
        }
    }
}