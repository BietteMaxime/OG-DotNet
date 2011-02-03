using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View
{
    public interface IViewTargetResultModel
    {
        IEnumerable<string> CalculationConfigurationNames { get; }
        IDictionary<string, ComputedValue> GetValues(string calcConfigurationName);
    }
}