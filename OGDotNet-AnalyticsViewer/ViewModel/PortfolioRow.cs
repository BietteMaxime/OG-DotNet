//-----------------------------------------------------------------------
// <copyright file="PortfolioRow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Engine;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PortfolioRow : DynamicRow
    {
        private readonly ComputationResultsTables.TreeNode _treeNode;
        private string _name;

        public PortfolioRow(ComputationResultsTables.TreeNode treeNode)
        {
            _treeNode = treeNode;
        }

        public string PositionName
        {
            get
            {
                _name = _name ?? string.Format("{0} {1}", new string('-', _treeNode.Depth), _treeNode.Name);
                return _name;
            }
        }

        public ComputationTargetSpecification ComputationTargetSpecification
        {
            get { return _treeNode.ComputationTargetSpecification; }
        }

        public ISecurity Security
        {
            get { return _treeNode.Security(); }
        }
    }
}