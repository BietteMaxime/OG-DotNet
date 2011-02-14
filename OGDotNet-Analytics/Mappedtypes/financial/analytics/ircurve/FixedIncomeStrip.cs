using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class FixedIncomeStrip
    {
        public StripInstrumentType InstrumentType;
        public Tenor CurveNodePointTime;
        public String ConventionName;
        public int NthFutureFromTenor;

        public static FixedIncomeStrip FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("type", InstrumentType.ToString());
            s.WriteInline(a, "tenor", CurveNodePointTime);
            a.Add("conventionName", ConventionName);
            a.Add("numFutures", NthFutureFromTenor);
        }
    }
}