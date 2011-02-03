using System.Collections.Generic;
using OGDotNet_Analytics.Mappedtypes.engine.Value;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    public interface IViewTargetResultModel
    {
        IEnumerable<string> CalculationConfigurationNames { get; }
        IDictionary<string, ComputedValue> GetValues(string calcConfigurationName);
    }
}