using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using OGDotNet_Analytics.Mappedtypes.Core.Position;
using OGDotNet_Analytics.Mappedtypes.engine;
using OGDotNet_Analytics.Mappedtypes.engine.View;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Model.Resources;

namespace OGDotNet_Analytics.View
{
    public class ComputationResultsTables : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        private readonly ViewDefinition _viewDefinition;
        private readonly IPortfolio _portfolio;
        private readonly RemoteSecuritySource _remoteSecuritySource;
        private readonly List<string> _portfolioColumns;
        private readonly List<string> _primitiveColumns;

        #region PropertyBag
        private List<PortfolioRow> _portfolioRows = new List<PortfolioRow>();
        private readonly Dictionary<UniqueIdentifier, PrimitiveRow> _primitiveRows = new Dictionary<UniqueIdentifier, PrimitiveRow>();

        public ComputationResultsTables(ViewDefinition viewDefinition, IPortfolio portfolio, RemoteSecuritySource remoteSecuritySource)
        {
            _viewDefinition = viewDefinition;
            _portfolio = portfolio;
            _remoteSecuritySource = remoteSecuritySource;
            _portfolioColumns = GetPortfolioColumns(viewDefinition).OrderBy(c=>c).ToList();
            _primitiveColumns = GetPrimitiveColumns(viewDefinition).OrderBy(c=>c).ToList();
        }


        public void Update(ViewComputationResultModel results, CancellationToken cancellationToken)
        {
            var valueIndex = Indexvalues(results);

            cancellationToken.ThrowIfCancellationRequested();
            _portfolioRows= BuildPortfolioRows(_viewDefinition, _portfolio, valueIndex, _remoteSecuritySource).ToList();
            InvokePropertyChanged("PortfolioRows");


            cancellationToken.ThrowIfCancellationRequested();

            bool rowsChanged = MergeUpdatePrimitiveRows(valueIndex);

            if (rowsChanged)
                InvokePropertyChanged("PrimitiveRows");//TODO this could be an ObservableCollection for extra niceness

        }

        private bool MergeUpdatePrimitiveRows(Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex)
        {
            bool rowsChanged = false;
            
            var targets = _viewDefinition.CalculationConfigurationsByName.SelectMany(conf => conf.Value.SpecificRequirements).Select(s => s.ComputationTargetIdentifier).Distinct();
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
                    foreach (var valueReq in configuration.Value.SpecificRequirements.Where(r => r.ComputationTargetType == ComputationTargetType.PRIMITIVE && r.ComputationTargetIdentifier == row.TargetId))
                    {
                        var key = new Tuple<UniqueIdentifier, string, string>(row.TargetId.ToLatest(), valueReq.ValueName, configuration.Key);

                        object value;
                        if (valueIndex.TryGetValue(key, out value))
                        {
                            values.Add(GetColumnHeading(configuration.Key, valueReq.ValueName), value);
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


        public void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
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
                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.ComputationTargetType == ComputationTargetType.PRIMITIVE))
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

        private static IEnumerable<TreeNode> GetNodes(IPortfolio portfolio, RemoteSecuritySource remoteSecuritySource)
        {
            return  GetNodesInner(portfolio.Root, remoteSecuritySource).ToList();
        }

        private static IEnumerable<TreeNode> GetNodesInner(PortfolioNode node, RemoteSecuritySource remoteSecuritySource)
        {
            yield return  new TreeNode(UniqueIdentifier.Parse(node.Identifier), node.Name);
            foreach (var position in node.Positions)
            {
                var securityNames = remoteSecuritySource.GetSecurities(position.SecurityKey).Select(s => s.Name).Distinct().ToList();
                if (securityNames.Count != 1)
                {
                    throw new ArgumentException();
                }

                string securityName = securityNames[0];
                yield return new TreeNode(position.Identifier, String.Format("{0} ({1})", securityName, position.Quantity));
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetNodesInner(portfolioNode, remoteSecuritySource))
                {
                    yield return treeNode;
                }
            }
        }

        private class TreeNode
        {
            private readonly UniqueIdentifier _identifier;
            private readonly string _name;

            public TreeNode(UniqueIdentifier identifier, string name)
            {
                _identifier = identifier;
                _name = name;
            }

            public UniqueIdentifier Identifier
            {
                get { return _identifier; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        private static IEnumerable<PortfolioRow> BuildPortfolioRows(ViewDefinition viewDefinition, IPortfolio portfolio, Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex, RemoteSecuritySource remoteSecuritySource)
        {
            if (portfolio == null)
                yield break;
            foreach (var position in GetNodes(portfolio, remoteSecuritySource))
            {
                var values = new Dictionary<string, object>();

                foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var req in configuration.Value.PortfolioRequirementsBySecurityType)
                    {
                        //TODO respect security type
                        foreach (var portfolioReq in req.Value.Properties["portfolioRequirement"])
                        {
                            string header = String.Format("{0}/{1}", configuration.Key, portfolioReq);
                            
                            var key = new Tuple<UniqueIdentifier, string, string>(position.Identifier.ToLatest(), portfolioReq, configuration.Key);
                            object value;
                            if (valueIndex.TryGetValue(key, out value))
                            {
                                values.Add(header, value);
                            }
                            else
                            {
                                values.Add(header, "undefined");
                            }
                        }
                    }
                }


                yield return new PortfolioRow(position.Identifier, position.Name, values);
            }
        }

        private static Dictionary<Tuple<UniqueIdentifier, string, string>, object> Indexvalues(ViewComputationResultModel results)
        {
            var valueIndex = new Dictionary<Tuple<UniqueIdentifier, string, string>, object>();
            foreach (var result in results.AllResults)
            {

                switch (result.ComputedValue.Specification.TargetSpecification.Type)
                {
                    case ComputationTargetType.PRIMITIVE:
                    case ComputationTargetType.POSITION:
                    case ComputationTargetType.PORTFOLIO_NODE:
                        valueIndex.Add(new Tuple<UniqueIdentifier, string, string>(
                                           result.ComputedValue.Specification.TargetSpecification.Uid.ToLatest(),
                                           result.ComputedValue.Specification.ValueName, result.CalculationConfiguration),
                                       result.ComputedValue.Value);
                        break;
                    case ComputationTargetType.TRADE:
                    case ComputationTargetType.SECURITY:
                        //TODO throw new NotImplementedException();
                        break;
                }
            }
            return valueIndex;
        }
    }
}
