//-----------------------------------------------------------------------
// <copyright file="InterpolatedYieldCurveSpecificationWithSecurities.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using Fudge.Types;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class InterpolatedYieldCurveSpecificationWithSecurities
    {
        private readonly DateTimeOffset _curveDate;
        private readonly string _name;
        private readonly Currency _currency;
        private readonly List<FixedIncomeStripWithSecurity> _strips;

        private InterpolatedYieldCurveSpecificationWithSecurities(DateTimeOffset curveDate, string name, Currency currency, List<FixedIncomeStripWithSecurity> strips)
        {
            _curveDate = curveDate;
            _name = name;
            _currency = currency;
            _strips = strips;
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

        public IEnumerable<FixedIncomeStripWithSecurity> Strips
        {
            get { return _strips; }
        }

        public static InterpolatedYieldCurveSpecificationWithSecurities FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new InterpolatedYieldCurveSpecificationWithSecurities(
                new DateTimeOffset( 
                ((FudgeDate) ffc.GetValue("curveDate")).ToDateTime()),
                (string) ffc.GetValue("name"),
                ffc.GetValue<Currency>("currency"),
                ffc.GetAllByName("resolvedStrips").Select(deserializer.FromField<FixedIncomeStripWithSecurity>).ToList()
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}