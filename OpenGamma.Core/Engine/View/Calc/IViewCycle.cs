// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.View.Compilation;
using OpenGamma.Id;

namespace OpenGamma.Engine.View.Calc
{
    public interface IViewCycle : IUniqueIdentifiable
    {
        ICompiledViewDefinitionWithGraphs GetCompiledViewDefinition();
        IViewComputationResultModel GetResultModel();
        ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery);

        UniqueId GetViewProcessId();

        ViewCycleState GetState();
        long GetDurationNanos();
    }
}
