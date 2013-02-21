// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageablePortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Master.Portfolio
{
    [FudgeSurrogate(typeof(ManageablePortfolioNodeBuilder))]
    public class ManageablePortfolioNode
    {
        private readonly List<ManageablePortfolioNode> _childNodes = new List<ManageablePortfolioNode>();
        private readonly List<ObjectId> _positionIds = new List<ObjectId>(); 

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageablePortfolioNode" /> class.
        /// </summary>
        public ManageablePortfolioNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageablePortfolioNode" /> class, specifying the name.
        /// </summary>
        /// <param name="name">the name, not null</param>
        public ManageablePortfolioNode(string name)
        {
            ArgumentChecker.NotNull(name, "name");
            Name = name;
        }

        /// <summary>
        /// Adds a position object identifier to this node.
        /// </summary>
        /// <param name="positionId">the object identifier of the position, not null</param>
        public void AddPosition(IObjectIdentifiable positionId)
        {
            ArgumentChecker.NotNull(positionId, "positionId");
            PositionIds.Add(positionId.ObjectId);
        }

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        /// <param name="childNode">the child node, not null</param>
        public void AddChildNode(ManageablePortfolioNode childNode)
        {
            ArgumentChecker.NotNull(childNode, "childNode");
            ChildNodes.Add(childNode);
        }

        public UniqueId UniqueId { get; set; }
        public UniqueId ParentNodeId { get; set; }
        public UniqueId PortfolioId { get; set; }
        public string Name { get; set; }

        public IList<ManageablePortfolioNode> ChildNodes
        {
            get
            {
                return _childNodes;
            }
            set
            {
                _childNodes.Clear();
                _childNodes.AddRange(value);
            }
        }

        public IList<ObjectId> PositionIds
        {
            get
            {
                return _positionIds;
            }
            set
            {
                _positionIds.Clear();
                _positionIds.AddRange(value);
            }
        }
    }
}
