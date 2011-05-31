//-----------------------------------------------------------------------
// <copyright file="PortfolioImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    internal class PortfolioImpl : IPortfolio
    {
        private readonly PortfolioNode _root;
        private readonly UniqueIdentifier _identifier;
        private readonly string _name;

        public PortfolioImpl(PortfolioNode root, UniqueIdentifier identifier, string name)
        {
            _root = root;
            _identifier = identifier;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public PortfolioNode Root
        {
            get { return _root; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _identifier; }
        }
    }
}