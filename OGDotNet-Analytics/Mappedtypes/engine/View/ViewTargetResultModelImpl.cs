using System.Collections.Generic;
using OGDotNet_Analytics.Mappedtypes.engine.Value;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    internal class ViewTargetResultModelImpl : IViewTargetResultModel
    {
        private readonly Dictionary<string, Dictionary<string, ComputedValue>> _inner = new Dictionary<string, Dictionary<string, ComputedValue>>();
        public void AddAll(string key, Dictionary<string, ComputedValue> values)
        {
            _inner.Add(key, values);
        }

        public IEnumerable<string> CalculationConfigurationNames
        {
            get { return _inner.Keys; }
        }

        public IDictionary<string, ComputedValue> GetValues(string calcConfigurationName)
        {
            return _inner[calcConfigurationName];
        }
    }
}