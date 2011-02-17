using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class ComputationResultsTables : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly ISecuritySource _remoteSecuritySource;
        private readonly List<string> _portfolioColumns;
        private readonly List<string> _primitiveColumns;
        
        #region PropertyBag
        private List<PortfolioRow> _portfolioRows = new List<PortfolioRow>();
        private readonly Dictionary<UniqueIdentifier, PrimitiveRow> _primitiveRows = new Dictionary<UniqueIdentifier, PrimitiveRow>();
        private IEnumerable<TreeNode> _portfolioNodes;

        public ComputationResultsTables(ViewDefinition viewDefinition, IPortfolio portfolio, ISecuritySource remoteSecuritySource)
        {
            _viewDefinition = viewDefinition;
            _portfolio = portfolio;
            _remoteSecuritySource = remoteSecuritySource;
            _portfolioColumns = GetPortfolioColumns(viewDefinition).ToList();
            _primitiveColumns = GetPrimitiveColumns(viewDefinition).ToList();
        }


        public void Update(ViewComputationResultModel results, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _portfolioRows= BuildPortfolioRows(results).ToList();
            InvokePropertyChanged("PortfolioRows");


            cancellationToken.ThrowIfCancellationRequested();

            bool rowsChanged = MergeUpdatePrimitiveRows(results);

            if (rowsChanged)
                InvokePropertyChanged("PrimitiveRows");//TODO this could be an ObservableCollection for extra niceness

        }

        private bool MergeUpdatePrimitiveRows(ViewComputationResultModel results)
        {
            bool rowsChanged = false;
            
            var targets = _viewDefinition.CalculationConfigurationsByName.SelectMany(conf => conf.Value.SpecificRequirements).Select(s => s.TargetSpecification.Uid).Distinct();
            foreach (var target in targets.Where(target => !_primitiveRows.ContainsKey(target)))
            {
                _primitiveRows.Add(target, new PrimitiveRow(target));
                rowsChanged = true;
            }

            foreach (var row in _primitiveRows.Values)
            {
                var values = new Dictionary<string, object>();
                foreach (var configuration in _viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var valueReq in configuration.Value.SpecificRequirements.Where(r => r.TargetSpecification.Type == ComputationTargetType.Primitive && r.TargetSpecification.Uid == row.TargetId))
                    {
                        object result;
                        if (results.TryGetValue(configuration.Key, valueReq, out result ))
                        {
                            values.Add(GetColumnHeading(configuration.Key, valueReq.ValueName), result);
                        }
                    }
                }
                row.Update(values);
            }

            return rowsChanged;
        }


        public bool HavePortfolioRows { get { return PortfolioColumns.Count > 0; } }
        public bool HavePrimitiveRows { get { return PrimitiveColumns.Count > 0; } }

        public List<string> PortfolioColumns
        {
            get { return _portfolioColumns; }
        }

        public List<string> PrimitiveColumns
        {
            get { return _primitiveColumns; }
        }

        public List<PortfolioRow> PortfolioRows
        {
            get { return _portfolioRows; }
        }

        public List<PrimitiveRow> PrimitiveRows
        {
            get { return _primitiveRows.Values.OrderBy(r=>r.TargetName).ToList(); }
        }


        private void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        private static IEnumerable<string> GetPrimitiveColumns(ViewDefinition viewDefinition)
        {
            var valueNames = new HashSet<string>();
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.TargetSpecification.Type == ComputationTargetType.Primitive))
                {
                    valueNames.Add(GetColumnHeading(configuration.Key, req.ValueName));
                }
            }
            return valueNames;
        }

        private static IEnumerable<string> GetPortfolioColumns(ViewDefinition viewDefinition)
        {

            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                foreach (var valuePropertiese in configuration.Value.PortfolioRequirementsBySecurityType)
                {
                    foreach (var property in valuePropertiese.Value.Properties)
                    {
                        foreach (var p in property.Value)
                        {
                            yield return GetColumnHeading(configuration.Key, p);
                        }
                    }
                }
            }
        }

        private static string GetColumnHeading(string configuration, string valueName)
        {
            return String.Format("{0}/{1}", configuration, valueName);
        }

        
        private IEnumerable<TreeNode> GetPortfolioNodes()
        {
            //We cache these in order to cache security names
            _portfolioNodes = _portfolioNodes ?? GetPortfolioNodesInner(_portfolio.Root).ToList();
            return _portfolioNodes;
        }


        private IEnumerable<TreeNode> GetPortfolioNodesInner(PortfolioNode node)
        {
            yield return  new TreeNode(UniqueIdentifier.Parse(node.Identifier), node.Name, ComputationTargetType.PortfolioNode);
            foreach (var position in node.Positions)
            {
                yield return new TreeNode(position.Identifier, String.Format("{0} ({1})", GetSecurityName(position), position.Quantity), ComputationTargetType.Position);
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetPortfolioNodesInner(portfolioNode))
                {
                    yield return treeNode;
                }
            }
        }


        private string GetSecurityName(Position position)
        {
            return _remoteSecuritySource.GetSecurity(position.SecurityKey).Name;
        }

        private class TreeNode
        {
            private readonly UniqueIdentifier _identifier;
            private readonly string _name;
            private readonly ComputationTargetType _targetType;

            public TreeNode(UniqueIdentifier identifier, string name, ComputationTargetType targetType)
            {
                _identifier = identifier;
                _name = name;
                _targetType = targetType;
            }

            public string Name
            {
                get { return _name; }
            }

            public ComputationTargetSpecification ComputationTargetSpecification
            {
                get { return new ComputationTargetSpecification(_targetType, _identifier); }
            }
        }

        private IEnumerable<PortfolioRow> BuildPortfolioRows(ViewComputationResultModel results)
        {
            if (_portfolio == null)
                yield break;
            foreach (var position in GetPortfolioNodes())
            {
                var values = new Dictionary<string, object>();

                foreach (var configuration in _viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var req in configuration.Value.PortfolioRequirementsBySecurityType)
                    {
                        foreach (var portfolioReq in req.Value.Properties["portfolioRequirement"])
                        {
                            string header = String.Format("{0}/{1}", configuration.Key, portfolioReq);
                            
                            object result;
                            if (results.TryGetValue(configuration.Key, new ValueRequirement(portfolioReq, position.ComputationTargetSpecification), out result))
                            {
                                values.Add(header, result);
                            }
                            else
                            {
                                values.Add(header, null);
                            }
                        }
                    }
                }


                yield return new PortfolioRow(position.Name, values);
            }
        }
    }
}
