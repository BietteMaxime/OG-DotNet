// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexType.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;
using OpenGamma.Fudge;

namespace OpenGamma.Financial.Analytics.IRCurve
{
    [FudgeSurrogate(typeof(EnumBuilder<IndexType>))]
    public enum IndexType
    {
        // NOTE jonathan 2013-01-16 -- PLAT-3010, hence the FudgeValue attributes

        [FudgeValue("Libor")]
        Libor,

        [FudgeValue("Tibor")]
        Tibor,

        [FudgeValue("Euribor")]
        Euribor,

        Bbsw,

        [FudgeValue("Swap")]
        Swap
    }
}
