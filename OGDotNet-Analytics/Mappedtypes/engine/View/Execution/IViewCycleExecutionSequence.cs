using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public interface IViewCycleExecutionSequence
    {
        bool IsEmpty { get; }
        ViewCycleExecutionOptions Next { get; }
    }

    public class RealTimeViewCycleExecutionSequence : IViewCycleExecutionSequence
    {
        public bool IsEmpty
        {
            get { return false; }
        }

        public ViewCycleExecutionOptions Next
        {
            get { return new ViewCycleExecutionOptions(DateTimeOffset.Now, DateTimeOffset.Now); }
        }

        public static RealTimeViewCycleExecutionSequence FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new RealTimeViewCycleExecutionSequence();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(RealTimeViewCycleExecutionSequence));
        }
    }
}