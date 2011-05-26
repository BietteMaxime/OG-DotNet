//-----------------------------------------------------------------------
// <copyright file="FixedIncomeStrip.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class FixedIncomeStrip : IComparable<FixedIncomeStrip>
    {
        public StripInstrumentType InstrumentType;
        public Tenor CurveNodePointTime;
        public string ConventionName;
        public int NthFutureFromTenor;

        public static FixedIncomeStrip FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            FixedIncomeStrip ret = new FixedIncomeStrip();
            ret.InstrumentType = EnumBuilder<StripInstrumentType>.Parse(ffc.GetValue<string>("type"));
            ret.CurveNodePointTime = new Tenor(ffc.GetValue<string>("tenor"));
            ret.ConventionName = ffc.GetValue<string>("conventionName");
            if (ret.InstrumentType == StripInstrumentType.Future)
            {
                ret.NthFutureFromTenor = ffc.GetValue<int>("numFutures");
            }

            return ret;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("type", EnumBuilder<StripInstrumentType>.GetJavaName(InstrumentType));
            s.WriteInline(a, "tenor", CurveNodePointTime);
            a.Add("conventionName", ConventionName);
            if (InstrumentType == StripInstrumentType.Future)
            {
                a.Add("numFutures", NthFutureFromTenor);
            }
        }

        public int CompareTo(FixedIncomeStrip other)
        {
            int result = CurveNodePointTime.TimeSpan.CompareTo(other.CurveNodePointTime.TimeSpan);
            if (result != 0)
            {
                return result;
            }
            result = InstrumentType.CompareTo(other.InstrumentType);
            if (result != 0)
            {
                return result;
            }
            if (InstrumentType == StripInstrumentType.Future)
            {
                result = NthFutureFromTenor.CompareTo(other.NthFutureFromTenor);
            }
            return result;
        }
    }
}