// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePortfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Core.Position.Impl
{
    [FudgeSurrogate(typeof(PortfolioBuilder))]
    internal class SimplePortfolio : IPortfolio
    {
        private readonly SimplePortfolioNode _root;
        private readonly UniqueId _identifier;
        private readonly string _name;

        public SimplePortfolio(SimplePortfolioNode root, UniqueId identifier, string name)
        {
            _root = root;
            _identifier = identifier;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public IPortfolioNode Root
        {
            get { return _root; }
        }

        public UniqueId UniqueId
        {
            get { return _identifier; }
        }
    }
}