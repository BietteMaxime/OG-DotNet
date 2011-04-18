using System;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    public class BaseViewResultListener : IViewResultListener
    {
        private readonly Action<InMemoryViewComputationResultModel, InMemoryViewComputationResultModel> _cycleCompletedCallBack;
        private readonly Action<ICompiledViewDefinition> _viewDefnCompiledCallBack;
        private readonly Action<DateTimeOffset, Exception> _compilationFailedCallBack;
        private readonly Action<ViewCycleExecutionOptions, Exception> _cycleFailedCallBack;

        //NOTE: I want anonymous types really
        public BaseViewResultListener(Action<InMemoryViewComputationResultModel, InMemoryViewComputationResultModel> cycleCompletedCallBack) : this (cycleCompletedCallBack, _ => { } )
        {
        }

        public BaseViewResultListener(Action<InMemoryViewComputationResultModel, InMemoryViewComputationResultModel> cycleCompletedCallBack, Action<ICompiledViewDefinition> viewDefnCompiledCallBack)
            : this (cycleCompletedCallBack, viewDefnCompiledCallBack, (offset, exception) => { }, (options, exception1) => { })
        {
            _cycleCompletedCallBack = cycleCompletedCallBack;
            _viewDefnCompiledCallBack = viewDefnCompiledCallBack;
        }
        public BaseViewResultListener(
            Action<InMemoryViewComputationResultModel, InMemoryViewComputationResultModel> cycleCompletedCallBack,
            Action<ICompiledViewDefinition> viewDefnCompiledCallBack,
            Action<DateTimeOffset, Exception> compilationFailedCallBack,
            Action<ViewCycleExecutionOptions, Exception> cycleFailedCallBack
            )
        {
            _cycleCompletedCallBack = cycleCompletedCallBack;
            _viewDefnCompiledCallBack = viewDefnCompiledCallBack;
            _compilationFailedCallBack = compilationFailedCallBack;
            _cycleFailedCallBack = cycleFailedCallBack;
        }

        public void ViewDefinitionCompiled(ICompiledViewDefinition compiledViewDefinition)
        {
            _viewDefnCompiledCallBack(compiledViewDefinition);
        }

        public void ViewDefinitionCompilationFailed(DateTimeOffset valuationTime, Exception exception)
        {
            _compilationFailedCallBack(valuationTime, exception);
        }

        public void CycleCompleted(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
        {
            _cycleCompletedCallBack(fullResult, deltaResult);
        }

        public void CycleExecutionFailed(ViewCycleExecutionOptions executionOptions, Exception exception)
        {
            _cycleFailedCallBack(executionOptions, exception);
        }

        public void ProcessCompleted()
        {
        }

        public void PocessTerminated(bool executionInterrupted)
        {
        }
    }
}