using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinition
    {
        private readonly Currency _currency;
        private readonly String _name;
        private readonly String _interpolatorName;
        private readonly List<FixedIncomeStrip> _strips = new List<FixedIncomeStrip>();
        private readonly string _region;

        public YieldCurveDefinition(Currency currency, string region, string name, string interpolatorName)
        {
            this._currency = currency;
            this._name = name;
            this._interpolatorName = interpolatorName;
            this._region = region;
        }

        public void AddStrip(params FixedIncomeStrip[] newStrips)
        {
            foreach (var fixedIncomeStrip in newStrips)
            {
                this._strips.Add(fixedIncomeStrip);
            }
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string InterpolatorName
        {
            get { return _interpolatorName; }
        }

        public SortedSet<FixedIncomeStrip> Strips
        {
            get { return new SortedSet<FixedIncomeStrip>(_strips); }
        }

        public string Region
        {
            get { return _region; }
        }

        public static YieldCurveDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("currency",_currency.ISOCode);
            if (_region != null)
                a.Add("region",_region);
            a.Add("name",_name);
            a.Add("interpolatorName",_interpolatorName);
            foreach (var fixedIncomeStrip in Strips)
            {
                s.WriteInline(a, "strip", fixedIncomeStrip);
            }
        }
    }
}