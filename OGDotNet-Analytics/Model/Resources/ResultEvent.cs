//-----------------------------------------------------------------------
// <copyright file="ResultEvent.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.engine.View.client;
using OGDotNet.Mappedtypes.engine.View.listener;

namespace OGDotNet.Model.Resources
{
    public class ResultEvent : EventArgs
    {
        private readonly object _msg;

        public ResultEvent(object msg)
        {
            _msg = msg;
        }

        public object Msg
        {
            get { return _msg; }
        }

        public void ApplyTo(IViewResultListener resultListener)
        {
            var defnCompiled = _msg as ViewDefinitionCompiledCall;
            var compileFailedCall = _msg as ViewDefinitionCompilationFailedCall;
            var cycleCompletedCall = _msg as CycleCompletedCall;
            var cycleFailedCall = _msg as CycleExecutionFailedCall;

            var completedCall = _msg as ProcessCompletedCall;
            var terminatedCall = _msg as ProcessTerminatedCall;

            var exception = _msg as Exception;
            
            if (defnCompiled != null)
            {
                resultListener.ViewDefinitionCompiled(defnCompiled.CompiledViewDefinition);
            }
            else if (compileFailedCall != null)
            {
                resultListener.ViewDefinitionCompilationFailed(compileFailedCall.ValuationTime, compileFailedCall.Exception);
            }
            else if (cycleCompletedCall != null)
            {
                resultListener.CycleCompleted(cycleCompletedCall.FullResult, cycleCompletedCall.DeltaResult);
            }
            else if (cycleFailedCall != null)
            {
                resultListener.CycleExecutionFailed(cycleFailedCall.ExecutionOptions, cycleFailedCall.Exception);
            }
            else if (completedCall != null)
            {
                resultListener.ProcessCompleted();
            }
            else if (terminatedCall != null)
            {
                resultListener.ProcessTerminated(terminatedCall.ExecutionInterrupted);
            }
            else if (exception != null)
            {
                resultListener.ViewDefinitionCompilationFailed(DateTimeOffset.Now, new JavaException(exception.GetType().ToString(), exception.Message));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}