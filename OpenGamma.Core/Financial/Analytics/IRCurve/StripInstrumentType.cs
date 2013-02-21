// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StripInstrumentType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Financial.Analytics.IRCurve
{
    public enum StripInstrumentType
    {
        Libor, 
        Cash, 
        Fra, 
        Future, 
        BankersAcceptance, 
        Swap, 
        TenorSwap, 
        BasisSwap, 
        OisSwap, 
        Euribor, 
        Fra3M, 
        Fra6M, 
        Swap3M, 
        Swap6M, 
        Swap12M,
        Cdor, 
        Cibor,
        Stibor,
        SimpleZeroDeposit,
        PeriodicZeroDeposit,
        ContinuousZeroDeposit,
        Spread
    }
}