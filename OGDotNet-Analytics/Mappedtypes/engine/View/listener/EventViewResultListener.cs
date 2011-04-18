using System;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    public class EventViewResultListener : IViewResultListener
    {
        public event EventHandler<ViewDefinitionCompiledArgs> ViewDefinitionCompiled;
        public event EventHandler<ViewDefinitionCompilationFailedArgs> ViewDefinitionCompilationFailed;
        public event EventHandler<CycleCompletedArgs> CycleCompleted;
        public event EventHandler<CycleExecutionFailedArgs> CycleExecutionFailed;



        //TODO ProcessCompleted, PocessTerminated


        void IViewResultListener.ViewDefinitionCompiled(ICompiledViewDefinition compiledViewDefinition)
        {
            InvokeViewDefinitionCompiled(new ViewDefinitionCompiledArgs(compiledViewDefinition));
        }

        void IViewResultListener.ViewDefinitionCompilationFailed(DateTimeOffset valuationTime, Exception exception)
        {
            InvokeViewDefinitionCompilationFailed(new ViewDefinitionCompilationFailedArgs(valuationTime, exception));
        }

        void IViewResultListener.CycleCompleted(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
        {
            InvokeCycleCompleted(new CycleCompletedArgs(fullResult, deltaResult));
        }

        void IViewResultListener.CycleExecutionFailed(ViewCycleExecutionOptions executionOptions, Exception exception)
        {
            InvokeCycleExecutionFailed(new CycleExecutionFailedArgs(executionOptions, exception));
        }

        public void ProcessCompleted()
        {//TODO
        }

        public void ProcessTerminated(bool executionInterrupted)
        {//TODO
        }


        private void InvokeViewDefinitionCompiled(ViewDefinitionCompiledArgs e)
        {
            EventHandler<ViewDefinitionCompiledArgs> handler = ViewDefinitionCompiled;
            if (handler != null) handler(this, e);
        }

        private void InvokeViewDefinitionCompilationFailed(ViewDefinitionCompilationFailedArgs e)
        {
            EventHandler<ViewDefinitionCompilationFailedArgs> handler = ViewDefinitionCompilationFailed;
            if (handler != null) handler(this, e);
        }

        private void InvokeCycleCompleted(CycleCompletedArgs e)
        {
            EventHandler<CycleCompletedArgs> handler = CycleCompleted;
            if (handler != null) handler(this, e);
        }
        public void InvokeCycleExecutionFailed(CycleExecutionFailedArgs e)
        {
            EventHandler<CycleExecutionFailedArgs> handler = CycleExecutionFailed;
            if (handler != null) handler(this, e);
        }
    }

    public class CycleExecutionFailedArgs : EventArgs
    {
        private readonly ViewCycleExecutionOptions _executionOptions;
        private readonly Exception _exception;

        public CycleExecutionFailedArgs(ViewCycleExecutionOptions executionOptions, Exception exception)
        {
            _executionOptions = executionOptions;
            _exception = exception;
        }

        public ViewCycleExecutionOptions ExecutionOptions
        {
            get { return _executionOptions; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }

    public class CycleCompletedArgs : EventArgs
    {
        private readonly InMemoryViewComputationResultModel _fullResult;
        private readonly InMemoryViewComputationResultModel _deltaResult;

        public CycleCompletedArgs(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
        {
            _fullResult = fullResult;
            _deltaResult = deltaResult;
        }

        public InMemoryViewComputationResultModel FullResult
        {
            get { return _fullResult; }
        }

        public InMemoryViewComputationResultModel DeltaResult
        {
            get { return _deltaResult; }
        }
    }

    public class ViewDefinitionCompilationFailedArgs : EventArgs
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly Exception _exception;

        public ViewDefinitionCompilationFailedArgs(DateTimeOffset valuationTime, Exception exception)
        {
            _valuationTime = valuationTime;
            _exception = exception;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }

    public class ViewDefinitionCompiledArgs : EventArgs
    {
        private readonly ICompiledViewDefinition _compiledViewDefinition;

        public ViewDefinitionCompiledArgs(ICompiledViewDefinition compiledViewDefinition)
        {
            _compiledViewDefinition = compiledViewDefinition;
        }

        public ICompiledViewDefinition CompiledViewDefinition
        {
            get { return _compiledViewDefinition; }
        }
    }
}