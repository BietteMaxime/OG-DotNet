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
using System.Text;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class ColumnHeader : IEquatable<ColumnHeader>
    {
        private readonly string _configuration;
        private readonly string _valueName;
        private readonly ValueProperties _requiredConstraints;

        public ColumnHeader(string configuration, string valueName, ValueProperties requiredConstraints)
        {
            _configuration = configuration;
            _valueName = valueName;

            _requiredConstraints = requiredConstraints;
        }

        public override string ToString()
        {
            return Text;
        }

        public string Text
        {
            get { return _configuration == "Default" ? _valueName : String.Format("{0}/{1}", _configuration, _valueName); }
        }
        public string ToolTip
        {
            get { return GetPropertiesString(_requiredConstraints); }
        }
        public ValueProperties RequiredConstraints
        {
            get { return _requiredConstraints; }
        }

        public string Configuration
        {
            get { return _configuration; }
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        private static String GetPropertiesString(ValueProperties constraints)
        {
            if (constraints.IsEmpty)
            {
                return "No constraints";
            }

            var sb = new StringBuilder();
            bool firstProperty = true;
            foreach (string propertyName in constraints.Properties)
            {
                if (propertyName == "Function")
                {
                    continue;
                }
                if (firstProperty)
                {
                    firstProperty = false;
                }
                else
                {
                    sb.Append("; \n");
                }
                sb.Append(propertyName).Append("=");
                ISet<String> propertyValues = constraints.GetValues(propertyName);
                if (propertyValues.Count() == 0)
                {
                    sb.Append("[empty]");
                }
                else if (propertyValues.Count() == 1)
                {
                    sb.Append(propertyValues.Single());
                }
                else
                {
                    sb.Append("(");
                    sb.Append(string.Join(", ", propertyValues));
                    sb.Append(")");
                }
            }
            return sb.ToString();
        }

        public bool Equals(ColumnHeader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._configuration, _configuration) && Equals(other._valueName, _valueName) && Equals(other._requiredConstraints, _requiredConstraints);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ColumnHeader)) return false;
            return Equals((ColumnHeader) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _configuration.GetHashCode();
                result = (result*397) ^ _valueName.GetHashCode();
                result = (result*397) ^ _requiredConstraints.GetHashCode();
                return result;
            }
        }
    }
    public class ComputationResultsTables
    {
        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly ISecuritySource _remoteSecuritySource;
        private readonly ICompiledViewDefinition _compiledViewDefinition;
        private readonly List<ColumnHeader> _portfolioColumns;
        private readonly List<ColumnHeader> _primitiveColumns;

        private readonly List<PrimitiveRow> _primitiveRows;

        private readonly List<PortfolioRow> _portfolioRows = new List<PortfolioRow>();

        private IEnumerable<TreeNode> _portfolioNodes;

        public ComputationResultsTables(ViewDefinition viewDefinition, IPortfolio portfolio, ISecuritySource remoteSecuritySource, ICompiledViewDefinition compiledViewDefinition)
        {
            _viewDefinition = viewDefinition;
            _portfolio = portfolio;
            _remoteSecuritySource = remoteSecuritySource;
            _compiledViewDefinition = compiledViewDefinition;
            _portfolioColumns = GetPortfolioColumns(viewDefinition, _compiledViewDefinition).ToList();
            _primitiveColumns = GetPrimitiveColumns(viewDefinition, _compiledViewDefinition).ToList();

            _primitiveRows = BuildPrimitiveRows().ToList();
            _portfolioRows = BuildPortfolioRows().ToList();
        }

        public void Update(CycleCompletedArgs results)
        {
            var delta = results.FullResult ?? results.DeltaResult;

            var indexedResults = delta.AllResults.ToLookup(v => v.ComputedValue.Specification.TargetSpecification.Uid);
            
            UpdatePortfolioRows(_portfolioRows, indexedResults);
            UpdatePrimitiveRows(_primitiveRows, indexedResults);
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
                var terminalOutputSpecifications = compiledViewDefinition.CompiledCalculationConfigurations[configuration.Key].TerminalOutputSpecifications.ToLookup(k=>Tuple.Create(k.ValueName, k.TargetSpecification));

                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.TargetSpecification.Type == ComputationTargetType.Primitive))
                {
                    var outputSpec = terminalOutputSpecifications[Tuple.Create(req.ValueName, req.TargetSpecification)].Where(s=>req.IsSatisfiedBy(s));
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
            //We cache these in order to cache security names
            _portfolioNodes = _portfolioNodes ?? GetPortfolioNodesInner(_portfolio.Root, 0).ToList();
            return _portfolioNodes;
        }

        private IEnumerable<TreeNode> GetPortfolioNodesInner(PortfolioNode node, int depth)
        {
            yield return new TreeNode(UniqueIdentifier.Parse(node.Identifier), node.Name, ComputationTargetType.PortfolioNode, null, depth);
            foreach (var position in node.Positions)
            {
                var security = _remoteSecuritySource.GetSecurity(position.SecurityKey);
                yield return new TreeNode(position.Identifier, string.Format("{0} ({1})", security.Name, position.Quantity), ComputationTargetType.Position, security, depth + 1);
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetPortfolioNodesInner(portfolioNode, depth + 1))
                {
                    yield return treeNode;
                }
            }
        }

        private class TreeNode
        {
            private readonly UniqueIdentifier _identifier;
            private readonly string _name;
            private readonly ComputationTargetType _targetType;
            private readonly Security _security;
            private readonly int _depth;

            public TreeNode(UniqueIdentifier identifier, string name, ComputationTargetType targetType, Security security, int depth)
            {
                _identifier = identifier;
                _name = name;
                _targetType = targetType;
                _security = security;
                _depth = depth;
            }

            public string Name
            {
                get { return _name; }
            }

            public Security Security
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
                var treeName = string.Format("{0} {1}", new string('-', position.Depth), position.Name);
                yield return new PortfolioRow(treeName, position.Security, position.ComputationTargetSpecification);
            }
        }

        private IEnumerable<PrimitiveRow> BuildPrimitiveRows()
        {
            var providedTargets = _compiledViewDefinition.CompiledCalculationConfigurations.SelectMany(c => c.Value.ComputationTargets.Select(t => t.UniqueId)).Distinct();
            var requestedTargets = _viewDefinition.CalculationConfigurationsByName.SelectMany(c => c.Value.SpecificRequirements.Select(r => r.TargetSpecification.Uid)).Distinct();
            return providedTargets.Intersect(requestedTargets).Select(t => new PrimitiveRow(t)).OrderBy(r => r.TargetName);
        }

        private static void UpdatePortfolioRows(IEnumerable<PortfolioRow> rows, ILookup<UniqueIdentifier, ViewResultEntry> indexedResults)
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

        private static void UpdatePrimitiveRows(IEnumerable<PrimitiveRow> rows, ILookup<UniqueIdentifier, ViewResultEntry> indexedResults)
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
    }
}
