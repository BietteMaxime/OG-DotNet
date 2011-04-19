// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CycleCompletedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    internal class CycleCompletedCall
    {
        private readonly InMemoryViewComputationResultModel _fullResult;
        private readonly InMemoryViewComputationResultModel _deltaResult;

        public CycleCompletedCall(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
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
}
