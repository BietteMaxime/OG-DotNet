//-----------------------------------------------------------------------
// <copyright file="PortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position
{
    [FudgeSurrogate(typeof(SimplePortfolioNodeBuilder))]
    public class PortfolioNode : IUniqueIdentifiable
    {
        private readonly UniqueId _uniqueId;
        private readonly string _name;
        private readonly IList<PortfolioNode> _subNodes;
        private readonly IList<IPosition> _positions;

        internal PortfolioNode(UniqueId uniqueId, string name, IList<PortfolioNode> subNodes, IList<IPosition> positions)
        {
            _uniqueId = uniqueId;
            _name = name;
            _subNodes = subNodes;
            _positions = positions;
        }

        public string Name
        {
            get { return _name; }
        }

        public IList<PortfolioNode> SubNodes
        {
            get { return _subNodes; }
        }

        public IList<IPosition> Positions
        {
            get { return _positions; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }
    }
}