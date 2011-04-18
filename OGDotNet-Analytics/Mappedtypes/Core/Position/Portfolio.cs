//-----------------------------------------------------------------------
// <copyright file="Portfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Core.Position
{
    [FudgeSurrogate(typeof(PortfolioBuilder))]
    public abstract class IPortfolio
    {
        public abstract string Identifier { get; }
        public abstract string Name { get; }
        public abstract PortfolioNode Root { get; }
    }
}