// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using OpenGamma.Id;

namespace OpenGamma.Core.Position
{
    public interface IPortfolioNode : IUniqueIdentifiable
    {
        IList<IPortfolioNode> ChildNodes { get; }
        IList<IPosition> Positions { get; }
        string Name { get; }
    }
}