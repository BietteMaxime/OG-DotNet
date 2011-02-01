using OGDotNet_Analytics.Mappedtypes.engine.Value;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    public class ViewResultEntry
    {

        private readonly string _calculationConfiguration;
        private readonly ComputedValue _computedValue;

        public ViewResultEntry(string calculationConfiguration, ComputedValue computedValue)
        {
            _calculationConfiguration = calculationConfiguration;
            _computedValue = computedValue;
        }

        public string CalculationConfiguration
        {
            get { return _calculationConfiguration; }
        }

        public ComputedValue ComputedValue
        {
            get { return _computedValue; }
        }
    }
}