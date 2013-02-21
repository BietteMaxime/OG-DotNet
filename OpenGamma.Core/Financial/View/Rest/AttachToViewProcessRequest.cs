// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachToViewProcessRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.View.Execution;
using OpenGamma.Id;

namespace OpenGamma.Financial.View.Rest
{
    class AttachToViewProcessRequest
    {
        private readonly UniqueId _viewDefinitionId;
        private readonly IViewExecutionOptions _executionOptions;
        private readonly bool _newBatchProcess;

        public AttachToViewProcessRequest(UniqueId viewDefinitionId, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            _viewDefinitionId = viewDefinitionId;
            _executionOptions = executionOptions;
            _newBatchProcess = newBatchProcess;
        }

        public UniqueId ViewDefinitionId
        {
            get { return _viewDefinitionId; }
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
