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
        private readonly ViewComputationResultModel _fullResult;
        private readonly ViewDeltaResultModel _deltaResult;

        public CycleCompletedCall(ViewComputationResultModel fullResult, ViewDeltaResultModel deltaResult)
        {
            if (fullResult == null && deltaResult == null)
            {
                throw new ArgumentNullException("fullResult", "Both results were null");
            }
            _fullResult = fullResult;
            _deltaResult = deltaResult;
        }

        public ViewComputationResultModel FullResult
        {
            get { return _fullResult; }
        }

        public ViewDeltaResultModel DeltaResult
        {
            get { return _deltaResult; }
        }
    }
}
