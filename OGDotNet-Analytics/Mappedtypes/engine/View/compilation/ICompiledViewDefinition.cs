// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompiledViewDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Engine.value;
using OGDotNet.Mappedtypes.Engine.View;

namespace OGDotNet.Mappedtypes.Engine.View.compilation
{
    public interface ICompiledViewDefinition
    {
        ViewDefinition ViewDefinition { get; }

        IPortfolio Portfolio { get; }

        Dictionary<ValueRequirement, ValueSpecification> MarketDataRequirements { get; }

        DateTimeOffset EarliestValidity { get; }
        DateTimeOffset LatestValidity { get; }
        Dictionary<string, ICompiledViewCalculationConfiguration> CompiledCalculationConfigurations { get; }
    }
}
