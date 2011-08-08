//-----------------------------------------------------------------------
// <copyright file="ComputationResultsTables.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class ComputationResultsTables
    {
        public event EventHandler ResultReceived;

        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly ICompiledViewDefinition _compiledViewDefinition;
        private readonly List<ColumnHeader> _portfolioColumns;
        private readonly List<ColumnHeader> _primitiveColumns;

        private readonly List<PrimitiveRow> _primitiveRows;

        private readonly List<PortfolioRow> _portfolioRows = new List<PortfolioRow>();

        readonly ActionFactory<ExternalIdBundle, ISecurity> _securityFactory;

        public ComputationResultsTables(ISecuritySource remoteSecuritySource, ICompiledViewDefinition compiledViewDefinition)
        {
            _securityFactory = new ActionFactory<ExternalIdBundle, ISecurity>(remoteSecuritySource.GetSecurity);

            _viewDefinition = compiledViewDefinition.ViewDefinition;
            _portfolio = compiledViewDefinition.Portfolio;
            _compiledViewDefinition = compiledViewDefinition;
            _portfolioColumns = GetPortfolioColumns(_viewDefinition, _compiledViewDefinition).ToList();
            _primitiveColumns = GetPrimitiveColumns(_viewDefinition, _compiledViewDefinition).ToList();

            _primitiveRows = BuildPrimitiveRows().ToList();
            _portfolioRows = BuildPortfolioRows().ToList();
        }

        public void Update(CycleCompletedArgs results)
        {
            var delta = results.FullResult ?? (IViewResultModel) results.DeltaResult;

            var indexedResults = delta.AllResults.ToLookup(v => v.ComputedValue.Specification.TargetSpecification.Uid);
            
            UpdatePortfolioRows(_portfolioRows, indexedResults);
            UpdatePrimitiveRows(_primitiveRows, indexedResults);

            InvokeResultReceived(EventArgs.Empty);
        }
        
        public bool HavePortfolioRows { get { return PortfolioColumns.Count > 0; } }
        public bool HavePrimitiveRows { get { return PrimitiveColumns.Count > 0; } }

        public List<ColumnHeader> PortfolioColumns
        {
            get { return _portfolioColumns; }
        }

        public List<ColumnHeader> PrimitiveColumns
        {
            get { return _primitiveColumns; }
        }

        public List<PortfolioRow> PortfolioRows
        {
            get { return _portfolioRows; }
        }

        public List<PrimitiveRow> PrimitiveRows
        {
            get { return _primitiveRows; }
        }

        private static IEnumerable<ColumnHeader> GetPrimitiveColumns(ViewDefinition viewDefinition, ICompiledViewDefinition compiledViewDefinition)
        {
            var columns = new HashSet<ColumnHeader>();
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                var terminalOutputSpecifications = compiledViewDefinition.CompiledCalculationConfigurations[configuration.Key].TerminalOutputSpecifications.ToLookup(k => Tuple.Create(k.ValueName, k.TargetSpecification));

                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.TargetSpecification.Type == ComputationTargetType.Primitive))
                {
                    var outputSpec = terminalOutputSpecifications[Tuple.Create(req.ValueName, req.TargetSpecification)].Where(s => req.IsSatisfiedBy(s));
                    if (outputSpec.Any())
                    {
                        columns.Add(new ColumnHeader(configuration.Key, req.ValueName, req.Constraints));
                    }
                }
            }
            return columns;
        }

        private static IEnumerable<ColumnHeader> GetPortfolioColumns(ViewDefinition viewDefinition, ICompiledViewDefinition compiledViewDefinition)
        {
            var columns = new HashSet<ColumnHeader>();
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                var terminalOutputSpecifications = compiledViewDefinition.CompiledCalculationConfigurations[configuration.Key].TerminalOutputSpecifications.ToLookup(k => Tuple.Create(k.ValueName));

                foreach (var secType in configuration.Value.PortfolioRequirementsBySecurityType)
                {
                    foreach (var req in secType.Value)
                    {
                        if (terminalOutputSpecifications.Contains(Tuple.Create(req.Item1)))
                        {
                            columns.Add(new ColumnHeader(configuration.Key, req.Item1, req.Item2));
                        }
                    }
                }
            }
            return columns;
        }

        private IEnumerable<TreeNode> GetPortfolioNodes()
        {
            return GetPortfolioNodesInner(_portfolio.Root, 0).ToList();
        }

        private IEnumerable<TreeNode> GetPortfolioNodesInner(PortfolioNode node, int depth)
        {
            yield return new TreeNode(node.UniqueId, node.Name, ComputationTargetType.PortfolioNode, depth);
            foreach (var position in node.Positions)
            {
                var security = _securityFactory.GetAction(position.SecurityKey);
                yield return new TreeNode(position.UniqueId, ComputationTargetType.Position, security, depth + 1, position.Quantity);
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetPortfolioNodesInner(portfolioNode, depth + 1))
                {
                    yield return treeNode;
                }
            }
        }

        public class TreeNode
        {
            private readonly UniqueId _identifier;
            private readonly ComputationTargetType _targetType;
            private readonly Func<ISecurity> _security;
            private readonly int _depth;
            private readonly long _quantity;

            private string _name;

            public TreeNode(UniqueId identifier, string name, ComputationTargetType targetType, int depth)
            {
                _identifier = identifier;
                _name = name;
                _targetType = targetType;
                _depth = depth;
            }

            public TreeNode(UniqueId identifier, ComputationTargetType targetType, Func<ISecurity> security, int depth, long quantity)
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
                    _name = _name ?? string.Format("{0} ({1})", _security().Name, _quantity);
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

        private IEnumerable<PortfolioRow> BuildPortfolioRows()
        {
            if (_portfolio == null)
                yield break;
            foreach (var position in GetPortfolioNodes())
            {
                yield return new PortfolioRow(position);
            }
        }

        private IEnumerable<PrimitiveRow> BuildPrimitiveRows()
        {
            var providedTargets = _compiledViewDefinition.CompiledCalculationConfigurations.SelectMany(c => c.Value.ComputationTargets.Select(t => t.UniqueId)).Distinct();
            var requestedTargets = _viewDefinition.CalculationConfigurationsByName.SelectMany(c => c.Value.SpecificRequirements.Select(r => r.TargetSpecification.Uid)).Distinct();
            return providedTargets.Intersect(requestedTargets).Select(t => new PrimitiveRow(t)).OrderBy(r => r.TargetName);
        }

        private static void UpdatePortfolioRows(IEnumerable<PortfolioRow> rows, ILookup<UniqueId, ViewResultEntry> indexedResults)
        {
            UpdateDynamicRows(rows, r => indexedResults[r.ComputationTargetSpecification.Uid].ToDictionary(
                GetColumnHeader,
                v => v.ComputedValue.Value)
                );
        }

        private static ColumnHeader GetColumnHeader(ViewResultEntry v)
        {
            return new ColumnHeader(v.CalculationConfiguration, v.ComputedValue.Specification.ValueName, v.ComputedValue.Specification.Properties);
        }

        private static void UpdatePrimitiveRows(IEnumerable<PrimitiveRow> rows, ILookup<UniqueId, ViewResultEntry> indexedResults)
        {
            UpdateDynamicRows(rows, r => indexedResults[r.TargetId].ToDictionary(
                GetColumnHeader,
                v => v.ComputedValue.Value)
                );
        }

        private static void UpdateDynamicRows<T>(IEnumerable<T> rows, Func<T, Dictionary<ColumnHeader, object>> updateSelector) where T : DynamicRow
        {
            foreach (var row in rows)
            {
                row.UpdateDynamicColumns(updateSelector(row));
            }
        }

        private void InvokeResultReceived(EventArgs e)
        {
            EventHandler handler = ResultReceived;
            if (handler != null) handler(this, e);
        }
    }
}
