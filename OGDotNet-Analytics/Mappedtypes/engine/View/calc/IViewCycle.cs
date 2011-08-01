//-----------------------------------------------------------------------
// <copyright file="IViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Engine.View.compilation;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Engine.View.calc
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
