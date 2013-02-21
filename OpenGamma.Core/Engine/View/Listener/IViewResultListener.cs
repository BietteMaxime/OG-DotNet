﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewResultListener.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using OpenGamma.Engine.View.Compilation;
using OpenGamma.Engine.View.Execution;

namespace OpenGamma.Engine.View.Listener
{
    /// <summary>
    /// A listener to the output of a view process. Calls to the listener are always made in
    ///  the sequence in which they occur; it may be assumed that the listener will not be used concurrently.
    /// </summary>
    public interface IViewResultListener
    {
        void ViewDefinitionCompiled(ICompiledViewDefinition compiledViewDefinition);

        void ViewDefinitionCompilationFailed(DateTimeOffset valuationTime, JavaException exception);

        void CycleCompleted(IViewComputationResultModel fullResult, IViewDeltaResultModel deltaResult);

        void CycleExecutionFailed(ViewCycleExecutionOptions executionOptions, JavaException exception);

        void ProcessCompleted();

        void ProcessTerminated(bool executionInterrupted);
    }
}