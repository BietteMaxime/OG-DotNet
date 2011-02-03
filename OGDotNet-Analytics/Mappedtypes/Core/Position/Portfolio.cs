using OGDotNet.Mappedtypes.Core.Position;

namespace OGDotNet.Mappedtypes.Core.Position
{
    public interface IPortfolio
    {
        string Identifier { get; }
        string Name { get;  }
        PortfolioNode Root { get;  }
    }
}