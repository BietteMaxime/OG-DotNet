//-----------------------------------------------------------------------
// <copyright file="InMemoryViewResultModelBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public abstract class InMemoryViewResultModelBase : IViewResultModel
    {
        private readonly UniqueIdentifier _viewProcessId;
        private readonly UniqueIdentifier _viewCycleId;
        private readonly DateTimeOffset _inputDataTimestamp;
        private readonly DateTimeOffset _resultTimestamp;
        private readonly IDictionary<string, ViewCalculationResultModel> _configurationMap;
        private readonly TimeSpan _calculationDuration;

        protected InMemoryViewResultModelBase(UniqueIdentifier viewProcessId, UniqueIdentifier viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap, TimeSpan calculationDuration)
        {
            _viewProcessId = viewProcessId;
            _viewCycleId = viewCycleId;
            _inputDataTimestamp = inputDataTimestamp;
            _resultTimestamp = resultTimestamp;
            _configurationMap = configurationMap;
            _calculationDuration = calculationDuration;
        }

        public DateTimeOffset ValuationTime { get { return _inputDataTimestamp; } }
        public DateTimeOffset ResultTimestamp { get { return _resultTimestamp; } }
        public TimeSpan CalculationDuration { get { return _calculationDuration; } }

        public UniqueIdentifier ViewProcessId
        {
            get { return _viewProcessId; }
        }

        public UniqueIdentifier ViewCycleId
        {
            get { return _viewCycleId; }
        }

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

        public bool TryGetComputedValue(string calculationConfiguration, ValueRequirement valueRequirement, out ComputedValue result)
        {
            result = null;

            ViewCalculationResultModel model;
            if (!_configurationMap.TryGetValue(calculationConfiguration, out model))
            {
                return false;
            }

            ISet<ComputedValue> values;
            if (!model.TryGetAllValues(valueRequirement.TargetSpecification, out values))
            {
                return false;
            }

            var computedValues = values.Where(v => valueRequirement.IsSatisfiedBy(v.Specification));
            result = computedValues.FirstOrDefault();
            return result != null;
        }

        public IEnumerable<ViewResultEntry> AllResults
        {
            get { return _configurationMap.SelectMany(config => config.Value.AllResults.Select(cv => new ViewResultEntry(config.Key, cv))); }
        }
    }
}