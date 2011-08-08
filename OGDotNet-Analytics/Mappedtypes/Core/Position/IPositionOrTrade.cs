//-----------------------------------------------------------------------
// <copyright file="IPositionOrTrade.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public interface IPositionOrTrade : IUniqueIdentifiable
    {
        ExternalIdBundle SecurityKey { get; }
        long Quantity { get; }
    }
}