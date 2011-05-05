// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CycleCompletedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    internal class CycleCompletedCall
    {
        private readonly InMemoryViewComputationResultModel _fullResult;
        private readonly InMemoryViewComputationResultModel _deltaResult;

        public CycleCompletedCall(InMemoryViewComputationResultModel fullResult, InMemoryViewComputationResultModel deltaResult)
        {
            if (fullResult == null && deltaResult == null)
            {
                throw new ArgumentNullException("fullResult","Both results were null");
            }
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
