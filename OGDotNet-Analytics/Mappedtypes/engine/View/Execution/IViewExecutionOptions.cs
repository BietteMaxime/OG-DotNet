// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Engine.View.Execution
{
    public interface IViewExecutionOptions
    {
        IViewCycleExecutionSequence ExecutionSequence { get; }
        int? MaxSuccessiveDeltaCycles { get; }
        ViewExecutionFlags Flags { get; }
        ViewCycleExecutionOptions DefaultExecutionOptions { get; }
    }
}
