// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessTerminatedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Engine.View.Listener
{
    class ProcessTerminatedCall
    {
        private readonly bool _executionInterrupted;

        public ProcessTerminatedCall(bool executionInterrupted)
        {
            _executionInterrupted = executionInterrupted;
        }

        public bool ExecutionInterrupted
        {
            get { return _executionInterrupted; }
        }
    }
}
