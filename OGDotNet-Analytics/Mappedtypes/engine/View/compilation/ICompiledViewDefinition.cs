using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public interface ICompiledViewDefinition
    {
        ViewDefinition ViewDefinition { get; }

        IPortfolio Portfolio { get; }

        Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements { get; }

        IList<string> OutputValueNames { get; }

        //TODO IEnumerable<ComputationTarget> ComputationTargets

        IList<string> SecurityTypes { get; }

        //TODO ValidFrom, ValidTo
    }
}
