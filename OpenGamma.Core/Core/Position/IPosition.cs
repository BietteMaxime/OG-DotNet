// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPosition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Core.Position
{
    [FudgeSurrogate(typeof(SimplePositionBuilder))]
    public interface IPosition : IPositionOrTrade
    {
        IEnumerable<ITrade> Trades { get; }
    }
}