// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPortfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Core.Position
{
    [FudgeSurrogate(typeof(PortfolioBuilder))]
    public interface IPortfolio : IUniqueIdentifiable
    {
        string Name { get; }
        IPortfolioNode Root { get; }
    }
}