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
