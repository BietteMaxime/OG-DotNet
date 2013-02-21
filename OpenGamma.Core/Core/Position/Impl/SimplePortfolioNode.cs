// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Fudge.Serialization;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Core.Position.Impl
{
    [FudgeSurrogate(typeof(SimplePortfolioNodeBuilder))]
    public class SimplePortfolioNode : IPortfolioNode
    {
        private readonly IList<IPortfolioNode> _childNodes;
        private readonly string _name;
        private readonly IList<IPosition> _positions;
        private readonly UniqueId _uniqueId;

        public SimplePortfolioNode(UniqueId uniqueId, string name, IList<IPortfolioNode> childNodes, 
                                   IList<IPosition> positions)
        {
            _uniqueId = uniqueId;
            _name = name;
            _childNodes = childNodes;
            _positions = positions;
        }

        public string Name
        {
            get { return _name; }
        }

        public IList<IPortfolioNode> ChildNodes
        {
            get { return _childNodes; }
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