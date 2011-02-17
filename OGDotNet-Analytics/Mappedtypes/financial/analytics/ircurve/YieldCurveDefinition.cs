using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinition
    {
        private readonly Currency _currency;
        private readonly String _name;
        private readonly String _interpolatorName;
        private readonly List<FixedIncomeStrip> _strips = new List<FixedIncomeStrip>();
        private Identifier _region;

        public YieldCurveDefinition(Currency currency, string name, string interpolatorName)
        {
            this._currency = currency;
            this._name = name;
            this._interpolatorName = interpolatorName;
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

        public Identifier Region
        {
            get { return _region; }
            set { _region = value; }
        }

        public static YieldCurveDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            Currency currency = null;
            string name = null;
            string interpolatorName=null;
            var strips = new List<FixedIncomeStrip>();
            Identifier region = null;
            foreach (var fudgeField in ffc.GetAllFields())
            {
                switch (fudgeField.Name)
                {
                    case "currency":
                        currency = Currency.GetInstance((string) fudgeField.Value);
                        break;
                    case "name":
                        name = (string) fudgeField.Value;
                        break;
                    case "interpolatorName":
                        interpolatorName = (string) fudgeField.Value;
                        break;
                    case "region":
                        region = Identifier.Parse((string) fudgeField.Value);
                        break;
                    case "strip":
                        strips.Add(deserializer.FromField<FixedIncomeStrip>(fudgeField));
                        break;
                }
            }

            var yieldCurveDefinition = new YieldCurveDefinition(currency,name,interpolatorName);
            yieldCurveDefinition.Region = region;
            yieldCurveDefinition.AddStrip(strips.ToArray());
            return yieldCurveDefinition;
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