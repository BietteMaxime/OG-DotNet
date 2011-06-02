//-----------------------------------------------------------------------
// <copyright file="ITrade.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    [FudgeSurrogate(typeof(TradeBuilder))]
    public interface ITrade : IPositionOrTrade
    {
        //TODO: the rest of this interface
        UniqueIdentifier ParentPositionId { get; }
        DateTimeOffset TradeDate { get; }
        ICounterparty Counterparty { get; }
    }
}
