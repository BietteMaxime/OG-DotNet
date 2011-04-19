//-----------------------------------------------------------------------
// <copyright file="ResultEvent.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
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
            var cycleCompletedCall = _msg as CycleCompletedCall;
            var defnCompiled = _msg as ViewDefinitionCompiledCall;
            var completedCall = _msg as ProcessCompletedCall;

            if (cycleCompletedCall != null)
            {
                resultListener.CycleCompleted(cycleCompletedCall.FullResult, cycleCompletedCall.DeltaResult);
            }
            else if (defnCompiled != null)
            {
                resultListener.ViewDefinitionCompiled(defnCompiled.CompiledViewDefinition);
            }
            else if (completedCall != null)
            {
                resultListener.ProcessCompleted();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}