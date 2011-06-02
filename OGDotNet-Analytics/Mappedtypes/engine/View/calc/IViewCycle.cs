//-----------------------------------------------------------------------
// <copyright file="IViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
    public interface IViewCycle : IUniqueIdentifiable
    {
        ICompiledViewDefinitionWithGraphs GetCompiledViewDefinition();
        IViewComputationResultModel GetResultModel();
        ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery);

        UniqueIdentifier GetViewProcessId();

        ViewCycleState GetState();
        long GetDurationNanos();
    }
}
