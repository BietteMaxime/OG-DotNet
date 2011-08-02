//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Money;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.Financial.Analytics.IRCurve
{
    /// <summary>
    /// TODO interpolator
    /// </summary>
    public class InterpolatedYieldCurveSpecification
    {
        private readonly DateTimeOffset _curveDate;
        private readonly string _name;
        private readonly Currency _currency;
        private readonly List<FixedIncomeStripWithIdentifier> _resolvedStrips;
        private readonly Identifier _region;

        public InterpolatedYieldCurveSpecification(DateTimeOffset curveDate, string name, Currency currency,  
                        List<FixedIncomeStripWithIdentifier> resolvedStrips, Identifier region)
        {
            _curveDate = curveDate;
            _name = name;
            _currency = currency;
            _resolvedStrips = resolvedStrips;
            _region = region;
        }

        public DateTimeOffset CurveDate
        {
            get { return _curveDate; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public IList<FixedIncomeStripWithIdentifier> ResolvedStrips
        {
            get { return _resolvedStrips; }
        }

        public Identifier Region
        {
            get { return _region; }
        }

        public static InterpolatedYieldCurveSpecification FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var curveDate = ffc.GetValue<DateTimeOffset>("curveDate");
            string name = ffc.GetString("name");
            var currency = ffc.GetValue<Currency>("currency");
            var region = Identifier.Parse(ffc.GetString("region"));
            var resolvedStripFields = ffc.GetAllByName("resolvedStrips");

            var resolvedStrips = resolvedStripFields.Select(deserializer.FromField<FixedIncomeStripWithIdentifierFudge>)
                .Select(s => new FixedIncomeStripWithIdentifier(EnumBuilder<StripInstrumentType>.Parse(s.Type), new Tenor(s.Tenor), Identifier.Parse(s.Identifier))).
                ToList();
            return new InterpolatedYieldCurveSpecification(curveDate, name, currency, resolvedStrips, region);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
        private class FixedIncomeStripWithIdentifierFudge
        {
            public string Type { get; set; }
            public string Tenor { get; set; }
            public string Identifier { get; set; }
        }
    }
}