// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YieldCurveDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Util.Money;

namespace OpenGamma.Financial.Analytics.IRCurve
{
    public class YieldCurveDefinition
    {
        private readonly Currency _currency;
        private readonly string _name;
        private readonly string _interpolatorName;
        private readonly List<FixedIncomeStrip> _strips = new List<FixedIncomeStrip>();
        private ExternalId _region;

        public YieldCurveDefinition(Currency currency, string name, string interpolatorName)
        {
            _currency = currency;
            _name = name;
            _interpolatorName = interpolatorName;
        }

        public void AddStrip(params FixedIncomeStrip[] newStrips)
        {
            foreach (var fixedIncomeStrip in newStrips)
            {
                _strips.Add(fixedIncomeStrip);
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

        public ExternalId Region
        {
            get { return _region; }
            set { _region = value; }
        }

        public static YieldCurveDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            Currency currency = null;
            string name = null;
            string interpolatorName = null;
            var strips = new List<FixedIncomeStrip>();
            ExternalId region = null;
            foreach (var fudgeField in ffc)
            {
                switch (fudgeField.Name)
                {
                    case "currency":
                        currency = Currency.Create((string)fudgeField.Value);
                        break;
                    case "name":
                        name = (string)fudgeField.Value;
                        break;
                    case "interpolatorName":
                        interpolatorName = (string)fudgeField.Value;
                        break;
                    case "region":
                        region = ExternalId.Parse((string)fudgeField.Value);
                        break;
                    case "strip":
                        strips.Add(deserializer.FromField<FixedIncomeStrip>(fudgeField));
                        break;
                }
            }

            var yieldCurveDefinition = new YieldCurveDefinition(currency, name, interpolatorName);
            yieldCurveDefinition.Region = region;
            yieldCurveDefinition.AddStrip(strips.ToArray());
            return yieldCurveDefinition;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("currency", _currency.ISOCode);
            if (_region != null)
                a.Add("region", _region);
            a.Add("name", _name);
            a.Add("interpolatorName", _interpolatorName);
            foreach (var fixedIncomeStrip in Strips)
            {
                s.WriteInline(a, "strip", fixedIncomeStrip);
            }
        }
    }
}