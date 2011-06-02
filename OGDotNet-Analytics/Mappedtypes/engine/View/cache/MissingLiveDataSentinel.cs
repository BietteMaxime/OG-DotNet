using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.View.cache
{
    public class MissingLiveDataSentinel
    {
        public static readonly MissingLiveDataSentinel Instance = new MissingLiveDataSentinel();

        private MissingLiveDataSentinel()
        {
        }

        public static MissingLiveDataSentinel FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return Instance;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof (MissingLiveDataSentinel));
        }
    }
}
