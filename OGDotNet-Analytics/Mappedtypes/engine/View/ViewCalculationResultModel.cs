using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewCalculationResultModelBuilder))]
    public class ViewCalculationResultModel
    {
        private readonly IDictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>> _map;

        public ViewCalculationResultModel(IDictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>> map)
        {
            _map = map;
        }

        public IEnumerable<ComputationTargetSpecification> AllTargets
        {
            get { return _map.Keys; }
        }

        public IEnumerable<ComputedValue> AllResults
        {
            get
            {
                return _map.SelectMany(kvp => kvp.Value.Select(kvp2 => kvp2.Value));
            }
        }

        public bool TryGetValue(ComputationTargetSpecification target, string valueName, out ComputedValue value)
        {
            IDictionary<string, ComputedValue> valueDict;
            if (!_map.TryGetValue(target, out valueDict))
            {
                value = null;
                return false;
            }
            return valueDict.TryGetValue(valueName, out value);
        }

        internal ViewCalculationResultModel ApplyDelta(ViewCalculationResultModel delta)
        {
            var dictionary = _map.Merge(delta._map, (a,b) => a.Merge(b));
            return new ViewCalculationResultModel(dictionary);
        }
    }
}