using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Mappedtypes.financial.view.rest
{
    class AttachToViewProcessRequest
    {
        private readonly string _viewDefinitionName;
        private readonly IViewExecutionOptions _executionOptions;
        private readonly bool _newBatchProcess;

        public AttachToViewProcessRequest(string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            _viewDefinitionName = viewDefinitionName;
            _executionOptions = executionOptions;
            _newBatchProcess = newBatchProcess;
        }

        public string ViewDefinitionName
        {
            get { return _viewDefinitionName; }
        }

        public IViewExecutionOptions ExecutionOptions
        {
            get { return _executionOptions; }
        }

        public bool NewBatchProcess
        {
            get { return _newBatchProcess; }
        }
    }
}
