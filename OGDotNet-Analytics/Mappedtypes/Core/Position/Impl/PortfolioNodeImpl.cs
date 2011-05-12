//-----------------------------------------------------------------------
// <copyright file="PortfolioNodeImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    internal class PortfolioNodeImpl : PortfolioNode
    {
        public PortfolioNodeImpl(string identifier, string name, IList<PortfolioNode> subNodes, IList<Position> positions) : base(identifier, name, subNodes, positions)
        {
        }
    }
}
