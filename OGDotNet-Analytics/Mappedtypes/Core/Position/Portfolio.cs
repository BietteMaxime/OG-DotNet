//-----------------------------------------------------------------------
// <copyright file="Portfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    [FudgeSurrogate(typeof(PortfolioBuilder))]
    public abstract class IPortfolio : IUniqueIdentifiable
    {
        public abstract string Name { get; }
        public abstract PortfolioNode Root { get; }
        public abstract UniqueIdentifier UniqueId { get; }
    }
}