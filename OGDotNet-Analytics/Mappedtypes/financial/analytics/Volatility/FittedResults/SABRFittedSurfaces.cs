//-----------------------------------------------------------------------
// <copyright file="SABRFittedSurfaces.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Math.Surface;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

namespace OGDotNet.Mappedtypes.Financial.Analytics.Volatility.FittedResults
{
    public class SABRFittedSurfaces
    {
        //TODO inverseJacobian

        private readonly InterpolatedDoublesSurface _alphaSurface;
        private readonly InterpolatedDoublesSurface _betaSurface;
        private readonly InterpolatedDoublesSurface _nuSurface;
        private readonly InterpolatedDoublesSurface _rhoSurface;
        private readonly Currency _currency;
        private readonly string _dayCountName;

        public SABRFittedSurfaces(InterpolatedDoublesSurface alphaSurface, InterpolatedDoublesSurface betaSurface, InterpolatedDoublesSurface nuSurface, InterpolatedDoublesSurface rhoSurface, Currency currency, string dayCountName)
        {
            _alphaSurface = alphaSurface;
            _betaSurface = betaSurface;
            _nuSurface = nuSurface;
            _rhoSurface = rhoSurface;
            _currency = currency;
            _dayCountName = dayCountName;
        }

        public InterpolatedDoublesSurface AlphaSurface
        {
            get { return _alphaSurface; }
        }

        public InterpolatedDoublesSurface BetaSurface
        {
            get { return _betaSurface; }
        }

        public InterpolatedDoublesSurface NuSurface
        {
            get { return _nuSurface; }
        }

        public InterpolatedDoublesSurface RhoSurface
        {
            get { return _rhoSurface; }
        }

        public Currency Currency
        {
            get { return _currency; }
        }

        public string DayCountName
        {
            get { return _dayCountName; }
        }

        public static SABRFittedSurfaces FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new SABRFittedSurfaces(
                deserializer.FromField<InterpolatedDoublesSurface>(ffc.GetByName("AlphaSurface")),
                deserializer.FromField<InterpolatedDoublesSurface>(ffc.GetByName("BetaSurface")),
                deserializer.FromField<InterpolatedDoublesSurface>(ffc.GetByName("NuSurface")),
                deserializer.FromField<InterpolatedDoublesSurface>(ffc.GetByName("RhoSurface")),
                Currency.Create(ffc.GetString("Currency")),
                ffc.GetString("DayCountName")
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}