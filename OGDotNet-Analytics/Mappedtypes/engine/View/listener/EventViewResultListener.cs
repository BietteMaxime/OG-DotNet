// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventViewResultListener.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    /// <summary>
    /// This class converts <see cref="IViewResultListener"/> into an event based model
    /// </summary>
    public class EventViewResultListener : IViewResultListener
    {
        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.ViewDefinitionCompiled"/>
        /// </summary>
        public event EventHandler<ViewDefinitionCompiledArgs> ViewDefinitionCompiled;

        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.ViewDefinitionCompilationFailed"/>
        /// </summary>
        public event EventHandler<ViewDefinitionCompilationFailedArgs> ViewDefinitionCompilationFailed;

        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.CycleCompleted"/>
        /// </summary>
        public event EventHandler<CycleCompletedArgs> CycleCompleted;

        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.CycleExecutionFailed"/>
        /// </summary>
        public event EventHandler<CycleExecutionFailedArgs> CycleExecutionFailed;

        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.ProcessCompleted"/>
        /// </summary>
        public event EventHandler ProcessCompleted;

        /// <summary>
        /// The event analogue of <see cref="IViewResultListener.ProcessTerminated"/>
        /// </summary>
        public event EventHandler<ProcessTerminatedArgs> ProcessTerminated;

        void IViewResultListener.ViewDefinitionCompiled(ICompiledViewDefinition compiledViewDefinition)
        {
            InvokeViewDefinitionCompiled(new ViewDefinitionCompiledArgs(compiledViewDefinition));
        }

        void IViewResultListener.ViewDefinitionCompilationFailed(DateTimeOffset valuationTime, JavaException exception)
        {
            InvokeViewDefinitionCompilationFailed(new ViewDefinitionCompilationFailedArgs(valuationTime, exception));
        }

        void IViewResultListener.CycleCompleted(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
        {
            InvokeCycleCompleted(new CycleCompletedArgs(fullResult, deltaResult));
        }

        void IViewResultListener.CycleExecutionFailed(ViewCycleExecutionOptions executionOptions, JavaException exception)
        {
            InvokeCycleExecutionFailed(new CycleExecutionFailedArgs(executionOptions, exception));
        }

        void IViewResultListener.ProcessCompleted()
        {
            InvokeProcessCompleted(EventArgs.Empty);
        }

        void IViewResultListener.ProcessTerminated(bool executionInterrupted)
        {
            InvokeProcessTerminated(new ProcessTerminatedArgs(executionInterrupted));
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
        private void InvokeCycleExecutionFailed(CycleExecutionFailedArgs e)
        {
            EventHandler<CycleExecutionFailedArgs> handler = CycleExecutionFailed;
            if (handler != null) handler(this, e);
        }

        private void InvokeProcessCompleted(EventArgs e)
        {
            EventHandler handler = ProcessCompleted;
            if (handler != null) handler(this, e);
        }

        private void InvokeProcessTerminated(ProcessTerminatedArgs e)
        {
            EventHandler<ProcessTerminatedArgs> handler = ProcessTerminated;
            if (handler != null) handler(this, e);
        }
    }

    public class ProcessTerminatedArgs : EventArgs
    {
        private readonly bool _executionInterrupted;

        public ProcessTerminatedArgs(bool executionInterrupted)
        {
            _executionInterrupted = executionInterrupted;
        }

        public bool ExecutionInterrupted
        {
            get { return _executionInterrupted; }
        }

        public override string ToString()
        {
            return string.Format("[ProcessTerminatedArgs: Interrupted {0}]", ExecutionInterrupted);
        }
    }

    public class CycleExecutionFailedArgs : EventArgs
    {
        private readonly ViewCycleExecutionOptions _executionOptions;
        private readonly JavaException _exception;

        public CycleExecutionFailedArgs(ViewCycleExecutionOptions executionOptions, JavaException exception)
        {
            _executionOptions = executionOptions;
            _exception = exception;
        }

        public ViewCycleExecutionOptions ExecutionOptions
        {
            get { return _executionOptions; }
        }

        public JavaException Exception
        {
            get { return _exception; }
        }

        public override string ToString()
        {
            return string.Format("[CycleExecutionFailedArgs: {0}]", Exception);
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
        private readonly JavaException _exception;

        public ViewDefinitionCompilationFailedArgs(DateTimeOffset valuationTime, JavaException exception)
        {
            _valuationTime = valuationTime;
            _exception = exception;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public JavaException Exception
        {
            get { return _exception; }
        }

        public override string ToString()
        {
            return string.Format("[ViewDefinitionCompilationFailedArgs: {0}  valued @ {1}]", Exception, ValuationTime);
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