using System;
using Fudge.Types;

namespace OGDotNet.Utils
{
    static class FudgeDateTimeExtensions
    {
        public static DateTimeOffset ToDateTimeOffsetWithDefault(this FudgeDateTime dt)
        {
            return (dt == null) ? default(DateTimeOffset) : dt.ToDateTimeOffset();
        }
    }
}
