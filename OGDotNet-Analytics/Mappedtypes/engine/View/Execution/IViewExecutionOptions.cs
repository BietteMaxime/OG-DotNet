// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public interface IViewExecutionOptions
    {
        IViewCycleExecutionSequence ExecutionSequence { get; }
        bool RunAsFastAsPossible { get; }
        bool LiveDataTriggerEnabled { get; }
        int? MaxSuccessiveDeltaCycles { get; }
        bool CompileOnly { get; }
    }
}
