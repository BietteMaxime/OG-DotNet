// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FixedIncomeStrip.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Util.Time;

namespace OpenGamma.Financial.Analytics.IRCurve
{
    public class FixedIncomeStrip : IComparable<FixedIncomeStrip>
    {
        public StripInstrumentType InstrumentType { get; set; }
        public Tenor CurveNodePointTime { get; set; }
        public string ConventionName { get; set; }
        public int NthFutureFromTenor { get; set; }
        public int PeriodsPerYear { get; set; }
        public Tenor PayTenor { get; set; }
        public Tenor ReceiveTenor { get; set; }
        public Tenor ResetTenor { get; set; }
        public IndexType PayIndexType { get; set; }
        public IndexType ReceiveIndexType { get; set; }
        public IndexType IndexType { get; set; }

        public static FixedIncomeStrip FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var result = new FixedIncomeStrip
                {
                    InstrumentType = EnumBuilder<StripInstrumentType>.Parse(ffc.GetString("type")),
                    CurveNodePointTime = ffc.GetValue<Tenor>("tenor"),
                    ConventionName = ffc.GetValue<string>("conventionName")
                };
            switch (result.InstrumentType)
            {
                case StripInstrumentType.Future:
                    result.NthFutureFromTenor = ffc.GetValue<int>("numFutures");
                    break;
                case StripInstrumentType.PeriodicZeroDeposit:
                    result.PeriodsPerYear = ffc.GetInt("periodsPerYear").Value;
                    break;
                case StripInstrumentType.Swap:
                case StripInstrumentType.OisSwap:
                    if (ffc.GetByName("resetTenor") != null)
                    {
                        result.ResetTenor = ffc.GetValue<Tenor>("resetTenor");
                        result.IndexType = (IndexType)deserializer.FromField(ffc.GetByName("indexType"), typeof(IndexType));
                    }
                    break;
                case StripInstrumentType.BasisSwap:
                    result.PayTenor = ffc.GetValue<Tenor>("payTenor");
                    result.ReceiveTenor = ffc.GetValue<Tenor>("receiveTenor");
                    result.PayIndexType = (IndexType)deserializer.FromField(ffc.GetByName("payIndexType"), typeof(IndexType));
                    result.ReceiveIndexType = (IndexType)deserializer.FromField(ffc.GetByName("receiveIndexType"), typeof(IndexType));
                    break;
            }
            return result;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("type", EnumBuilder<StripInstrumentType>.GetJavaName(InstrumentType));
            a.Add("tenor", CurveNodePointTime);
            a.Add("conventionName", ConventionName);
            switch (InstrumentType)
            {
                case StripInstrumentType.Future:
                    a.Add("numFutures", NthFutureFromTenor);
                    break;
                case StripInstrumentType.PeriodicZeroDeposit:
                    a.Add("periodsPerYear", PeriodsPerYear);
                    break;
                case StripInstrumentType.OisSwap:
                case StripInstrumentType.Swap:
                    if (ResetTenor != null)
                    {
                        a.Add("resetTenor", ResetTenor);
                        a.Add("indexType", EnumBuilder<IndexType>.GetJavaName(IndexType));
                    }
                    break;
                case StripInstrumentType.BasisSwap:
                    a.Add("payTenor", PayTenor);
                    a.Add("receiveTenor", ReceiveTenor);
                    a.Add("payIndexType", EnumBuilder<IndexType>.GetJavaName(PayIndexType));
                    a.Add("receiveIndexType", EnumBuilder<IndexType>.GetJavaName(ReceiveIndexType));
                    break;
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