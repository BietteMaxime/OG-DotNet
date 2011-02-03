using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewCalculationResultModelBuilder))]
    public class ViewCalculationResultModel
    {
        private readonly Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> _map;

        public ViewCalculationResultModel(Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> map)
        {
            _map = map;
        }

        public IEnumerable<ComputationTargetSpecification> AllTargets
        {
            get { return _map.Keys; }
        }

        public Dictionary<string, ComputedValue> this[ComputationTargetSpecification target]
        {
            get { return _map[target]; }
        }
    }
}