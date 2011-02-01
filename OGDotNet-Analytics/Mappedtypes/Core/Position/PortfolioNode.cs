using System.Collections.Generic;

namespace OGDotNet_Analytics.Mappedtypes.Core.Position
{
    public class PortfolioNode
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public IList<PortfolioNode> SubNodes { get; set; }
        public IList<Position> Positions { get; set; }
    }
}