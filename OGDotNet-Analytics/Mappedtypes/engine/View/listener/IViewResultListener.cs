using System;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    /// <summary>
    /// A listener to the output of a view process. Calls to the listener are always made in
    ///  the sequence in which they occur; it may be assumed that the listener will not be used concurrently.
    /// </summary>
    public interface IViewResultListener
    {
        void ViewDefinitionCompiled(ICompiledViewDefinition compiledViewDefinition);

        void ViewDefinitionCompilationFailed(DateTimeOffset valuationTime, Exception exception);

        void CycleCompleted(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult);

        void CycleExecutionFailed(ViewCycleExecutionOptions executionOptions, Exception exception);

        void ProcessCompleted();

        void ProcessTerminated(bool executionInterrupted);
    }
}
