//-----------------------------------------------------------------------
// <copyright file="AttachToViewProcessRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
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
