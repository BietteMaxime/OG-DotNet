using System;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class ViewCycleExecutionOptions
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly DateTimeOffset _inputDataTime;

        public ViewCycleExecutionOptions(DateTimeOffset valuationTime, DateTimeOffset inputDataTime)
        {
            _valuationTime = valuationTime;
            _inputDataTime = inputDataTime;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public DateTimeOffset InputDataTime
        {
            get { return _inputDataTime; }
        }
    }
}