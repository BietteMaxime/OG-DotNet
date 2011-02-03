using System;
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet_Analytics.Builders;
using OGDotNet_Analytics.Mappedtypes.engine.Value;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewComputationResultModelBuilder))]
    public class ViewComputationResultModel
    {
        private readonly string _viewName;
        private readonly FudgeDateTime _inputDataTimestamp;
        private readonly FudgeDateTime _resultTimestamp;
        private readonly IDictionary<string, ViewCalculationResultModel> _configurationMap;
        private readonly Dictionary<ComputationTargetSpecification, IViewTargetResultModel> _targetMap;
        private readonly List<ViewResultEntry> _allResults;

        public ViewComputationResultModel(string viewName, FudgeDateTime inputDataTimestamp, FudgeDateTime resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap, Dictionary<ComputationTargetSpecification, IViewTargetResultModel> targetMap, List<ViewResultEntry> allResults)
        {
            _viewName = viewName;
            _inputDataTimestamp = inputDataTimestamp;
            _resultTimestamp = resultTimestamp;
            _configurationMap = configurationMap;
            _targetMap = targetMap;
            _allResults = allResults;
        }

        public ICollection<ComputationTargetSpecification> AllTargets
        {
            get { return _targetMap.Keys; }
        }

        public ICollection<string> CalculationConfigurationNames { get { return _configurationMap.Keys; } }
        public ViewCalculationResultModel GetCalculationResult(string calcConfigurationName)
        {
            return _configurationMap[calcConfigurationName];
        }
        public IViewTargetResultModel GetTargetResult(ComputationTargetSpecification target)
        {
            return _targetMap[target];
        }
        public FudgeDateTime ValuationTime { get { return _inputDataTimestamp; } }
        public FudgeDateTime ResultTimestamp { get { return _resultTimestamp; } }
        public String ViewName { get { return _viewName; } }
        public IList<ViewResultEntry> AllResults { get { return _allResults; } }

        public ViewComputationResultModel ApplyDelta(ViewComputationResultModel delta)
        {
            var deltaResults = delta._allResults.ToDictionary(r => new Tuple<string, ValueSpecification>(r.CalculationConfiguration, r.ComputedValue.Specification), r => r);


            var results = new List<ViewResultEntry>(_allResults.Count);

            foreach (var pair in _allResults.Select(r => new Tuple<Tuple<string, ValueSpecification>, ViewResultEntry>(
                                                             new Tuple<string, ValueSpecification>(r.CalculationConfiguration, r.ComputedValue.Specification), r)))
            {
                var key = pair.Item1;
                var vre = pair.Item2;
                ViewResultEntry newValue;
                if (deltaResults.TryGetValue(key, out newValue))
                {
                    deltaResults.Remove(key);
                }
                else
                {
                    newValue = vre;
                }
                results.Add(newValue);
            }
            results.AddRange(deltaResults.Select(kvp => kvp.Value));

            return new ViewComputationResultModel(_viewName, delta._inputDataTimestamp, delta._resultTimestamp, _configurationMap, _targetMap, results);
        }
    }
}