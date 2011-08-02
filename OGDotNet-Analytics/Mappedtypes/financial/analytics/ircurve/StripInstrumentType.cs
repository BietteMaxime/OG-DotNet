//-----------------------------------------------------------------------
// <copyright file="StripInstrumentType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Financial.Analytics.IRCurve
{
    public enum StripInstrumentType
    {
        Libor,
        Cash,
        FRA,
        Future,
        Swap,
        TenorSwap,
        BasisSwap,
        OisSwap
    }
}