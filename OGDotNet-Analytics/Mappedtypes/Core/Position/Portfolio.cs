//-----------------------------------------------------------------------
// <copyright file="Portfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Core.Position
{
    public interface IPortfolio
    {
        string Identifier { get; }
        string Name { get;  }
        PortfolioNode Root { get;  }
    }
}