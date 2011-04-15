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
