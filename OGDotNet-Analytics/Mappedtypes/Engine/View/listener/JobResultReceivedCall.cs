//-----------------------------------------------------------------------
// <copyright file="JobResultReceivedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;

namespace OGDotNet.Mappedtypes.Engine.View.listener
{
    internal class JobResultReceivedCall
    {
        private readonly IViewComputationResultModel _fullResult;
        private readonly IViewDeltaResultModel _deltaResult;

        public JobResultReceivedCall(IViewComputationResultModel fullResult, IViewDeltaResultModel deltaResult)
        {
            if (fullResult == null && deltaResult == null)
            {
                throw new ArgumentNullException("fullResult", "Both results were null");
            }
            _fullResult = fullResult;
            _deltaResult = deltaResult;
        }

        public IViewComputationResultModel FullResult
        {
            get { return _fullResult; }
        }

        public IViewDeltaResultModel DeltaResult
        {
            get { return _deltaResult; }
        }
    }
}
