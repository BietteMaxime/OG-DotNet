// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Util.Money;

namespace OpenGamma.Financial.Analytics.IRCurve
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
        private readonly ExternalId _region;

        public InterpolatedYieldCurveSpecification(DateTimeOffset curveDate, string name, Currency currency,  
                        List<FixedIncomeStripWithIdentifier> resolvedStrips, ExternalId region)
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

        public ExternalId Region
        {
            get { return _region; }
        }

        public static InterpolatedYieldCurveSpecification FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var curveDate = ffc.GetValue<DateTimeOffset>("curveDate");
            string name = ffc.GetString("name");
            var currency = ffc.GetValue<Currency>("currency");
            var region = ExternalId.Parse(ffc.GetString("region"));
            var resolvedStripFields = ffc.GetAllByName("resolvedStrips");

            var resolvedStrips = resolvedStripFields.Select(deserializer.FromField<FixedIncomeStripWithIdentifier>)
                .ToList();
            return new InterpolatedYieldCurveSpecification(curveDate, name, currency, resolvedStrips, region);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}