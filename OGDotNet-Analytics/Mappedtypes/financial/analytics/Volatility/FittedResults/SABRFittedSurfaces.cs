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
using OGDotNet.Mappedtypes.Financial.Model.Volatility.Surface;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

namespace OGDotNet.Mappedtypes.Financial.Analytics.Volatility.FittedResults
{
    public class SABRFittedSurfaces
    {
        private readonly VolatilitySurface _alphaSurface;
        private readonly VolatilitySurface _betaSurface;
        private readonly VolatilitySurface _nuSurface;
        private readonly VolatilitySurface _rhoSurface;
        private readonly Currency _currency;
        private readonly string _dayCountName;

        public SABRFittedSurfaces(VolatilitySurface alphaSurface, VolatilitySurface betaSurface, VolatilitySurface nuSurface, VolatilitySurface rhoSurface, Currency currency, string dayCountName)
        {
            _alphaSurface = alphaSurface;
            _betaSurface = betaSurface;
            _nuSurface = nuSurface;
            _rhoSurface = rhoSurface;
            _currency = currency;
            _dayCountName = dayCountName;
        }

        public VolatilitySurface AlphaSurface
        {
            get { return _alphaSurface; }
        }

        public VolatilitySurface BetaSurface
        {
            get { return _betaSurface; }
        }

        public VolatilitySurface NuSurface
        {
            get { return _nuSurface; }
        }

        public VolatilitySurface RhoSurface
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
                deserializer.FromField<VolatilitySurface>(ffc.GetByName("AlphaSurface")),
                deserializer.FromField<VolatilitySurface>(ffc.GetByName("BetaSurface")),
                deserializer.FromField<VolatilitySurface>(ffc.GetByName("NuSurface")),
                deserializer.FromField<VolatilitySurface>(ffc.GetByName("RhoSurface")),
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