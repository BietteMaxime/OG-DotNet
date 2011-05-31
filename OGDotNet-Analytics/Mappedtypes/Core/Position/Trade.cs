//-----------------------------------------------------------------------
// <copyright file="Trade.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    [FudgeSurrogate(typeof(TradeBuilder))]
    public interface ITrade : IUniqueIdentifiable
    {
        //TODO: the rest of this interface
        UniqueIdentifier ParentPositionId { get; }
    }
}
