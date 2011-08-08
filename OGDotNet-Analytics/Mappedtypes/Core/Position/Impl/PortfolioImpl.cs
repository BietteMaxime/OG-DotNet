//-----------------------------------------------------------------------
// <copyright file="PortfolioImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    [FudgeSurrogate(typeof(PortfolioBuilder))]
    internal class PortfolioImpl : IPortfolio
    {
        private readonly PortfolioNode _root;
        private readonly UniqueId _identifier;
        private readonly string _name;

        public PortfolioImpl(PortfolioNode root, UniqueId identifier, string name)
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

        public UniqueId UniqueId
        {
            get { return _identifier; }
        }
    }
}