using System.Collections.Generic;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public class PortfolioNode
    {
        private readonly string _identifier;
        private readonly string _name;
        private readonly IList<PortfolioNode> _subNodes;
        private readonly IList<Position> _positions;

        public PortfolioNode(string identifier, string name, IList<PortfolioNode> subNodes, IList<Position> positions)
        {
            _identifier = identifier;
            _name = name;
            _subNodes = subNodes;
            _positions = positions;
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IList<PortfolioNode> SubNodes
        {
            get { return _subNodes; }
        }

        public IList<Position> Positions
        {
            get { return _positions; }
        }
    }
}