using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OGDotNet.Mappedtypes.engine.View.listener
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
