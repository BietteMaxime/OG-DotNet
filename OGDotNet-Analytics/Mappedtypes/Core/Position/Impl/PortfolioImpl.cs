using OGDotNet.Mappedtypes.Core.Position;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    internal class PortfolioImpl : IPortfolio
    {
        private readonly PortfolioNode _root;
        private readonly string _identifier;
        private readonly string _name;

        public PortfolioImpl(PortfolioNode root, string identifier, string name)
        {
            _root = root;
            _identifier = identifier;
            _name = name;
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public string Name
        {
            get { return _name; }
        }

        public PortfolioNode Root
        {
            get { return _root; }
        }
    }
}