//-----------------------------------------------------------------------
// <copyright file="PortfolioResultsTableBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public abstract class PortfolioResultsTableBase<T>
    {
        private readonly IPortfolio _portfolio;
        private readonly List<T> _portfolioRows = new List<T>();
        private readonly ActionFactory<ExternalIdBundle, ISecurity> _securityFactory;

        protected PortfolioResultsTableBase(ISecuritySource remoteSecuritySource, IPortfolio portfolio)
        {
            _securityFactory = new ActionFactory<ExternalIdBundle, ISecurity>(remoteSecuritySource.GetSecurity);
            _portfolio = portfolio;
            _portfolioRows = BuildPortfolioRows().ToList();
        }

        protected abstract T WrapPortfolioRow(PortfolioViewTreeNode viewTreeNode);

        private IEnumerable<PortfolioViewTreeNode> GetPortfolioNodes()
        {
            return GetPortfolioNodesInner(_portfolio.Root, 0).ToList();
        }

        private IEnumerable<PortfolioViewTreeNode> GetPortfolioNodesInner(PortfolioNode node, int depth)
        {
            yield return new PortfolioViewTreeNode(node.UniqueId, node.Name, ComputationTargetType.PortfolioNode, depth);
            foreach (var position in node.Positions)
            {
                var security = _securityFactory.GetAction(position.SecurityKey);
                yield return new PortfolioViewTreeNode(position.UniqueId, ComputationTargetType.Position, security, depth + 1, position.Quantity);
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetPortfolioNodesInner(portfolioNode, depth + 1))
                {
                    yield return treeNode;
                }
            }
        }

        protected IEnumerable<T> BuildPortfolioRows()
        {
            if (_portfolio == null)
                return new T[] { };
            return GetPortfolioNodes().Select(WrapPortfolioRow);
        }

        public List<T> PortfolioRows
        {
            get { return _portfolioRows; }
        }
    }

    public class PortfolioViewTreeNode
    {
        private readonly UniqueId _identifier;
        private readonly ComputationTargetType _targetType;
        private readonly Func<ISecurity> _security;
        private readonly int _depth;
        private readonly decimal _quantity;

        private string _name;

        public PortfolioViewTreeNode(UniqueId identifier, string name, ComputationTargetType targetType, int depth)
        {
            _identifier = identifier;
            _name = name;
            _targetType = targetType;
            _depth = depth;
        }

        public PortfolioViewTreeNode(UniqueId identifier, ComputationTargetType targetType, Func<ISecurity> security, int depth, decimal quantity)
        {
            _identifier = identifier;
            _targetType = targetType;
            _security = security;
            _depth = depth;
            _quantity = quantity;
        }

        public string Name
        {
            get
            {
                _name = _name ?? String.Format("{0} ({1})", _security().Name, _quantity);
                return _name;
            }
        }

        public Func<ISecurity> Security
        {
            get { return _security; }
        }

        public ComputationTargetSpecification ComputationTargetSpecification
        {
            get { return new ComputationTargetSpecification(_targetType, _identifier); }
        }

        public int Depth
        {
            get { return _depth; }
        }
    }
}