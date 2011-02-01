using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet_Analytics.Builders;
using OGDotNet_Analytics.Mappedtypes.engine.Value;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewCalculationResultModelBuilder))]
    public class ViewCalculationResultModel
    {
        private readonly Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> _map;

        public ViewCalculationResultModel(Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> map)
        {
            _map = map;
        }

        public ICollection<ComputationTargetSpecification> getAllTargets()
        {
            return _map.Keys;
        }

        public Dictionary<string, ComputedValue> this[ComputationTargetSpecification target]
        {
            get { return _map[target]; }
        }
    }
}