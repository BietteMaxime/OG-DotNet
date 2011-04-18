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
    public class CycleCompletedCall
    {
        private readonly InMemoryViewComputationResultModel _fullResult;

        public CycleCompletedCall(InMemoryViewComputationResultModel fullResult)
        {
            if (fullResult == null)
            {
                throw new NotImplementedException();
            }
            _fullResult = fullResult;
        }

        public InMemoryViewComputationResultModel FullResult
        {
            get { return _fullResult; }
        }
    }
}
