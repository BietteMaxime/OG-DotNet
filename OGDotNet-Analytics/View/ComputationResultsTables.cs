using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet_Analytics.Mappedtypes.Core.Position;
using OGDotNet_Analytics.Mappedtypes.engine;
using OGDotNet_Analytics.Mappedtypes.engine.View;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Model.Resources;

namespace OGDotNet_Analytics.View
{
    public class ComputationResultsTables
    {
        #region PropertyBag
        private readonly List<Row> _portfolioRows;
        private readonly List<PrimitiveRow> _primitiveRows;

        private ComputationResultsTables(List<Row> portfolioRows, List<PrimitiveRow> primitiveRows)
        {
            _portfolioRows = portfolioRows;
            _primitiveRows = primitiveRows;
        }

        public List<Row> PortfolioRows
        {
            get { return _portfolioRows; }
        }

        public List<PrimitiveRow> PrimitiveRows
        {
            get { return _primitiveRows; }
        }

        #endregion



        public static ComputationResultsTables Build(ViewComputationResultModel results, CancellationToken cancellationToken, ViewDefinition viewDefinition, Portfolio portfolio, RemoteSecuritySource remoteSecuritySource)
        {
            var valueIndex = Indexvalues(results);

            cancellationToken.ThrowIfCancellationRequested();
            var portfolioRows = BuildPortfolioRows(viewDefinition, portfolio, valueIndex, remoteSecuritySource).ToList();

            cancellationToken.ThrowIfCancellationRequested();
            var primitiveRows = BuildPrimitiveRows(viewDefinition, valueIndex).ToList();

            return new ComputationResultsTables(portfolioRows, primitiveRows);
        }

        private static IEnumerable<PrimitiveRow> BuildPrimitiveRows(ViewDefinition viewDefinition, Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex)
        {
            var targets = viewDefinition.CalculationConfigurationsByName.SelectMany(conf => conf.Value.SpecificRequirements).Select(s => s.ComputationTargetIdentifier).Distinct();
            foreach (var target in targets)
            {
                var values = new Dictionary<string, object>();
                foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var valueReq in configuration.Value.SpecificRequirements.Where(r =>r.ComputationTargetType == ComputationTargetType.PRIMITIVE && r.ComputationTargetIdentifier == target))
                    {
                        var key = new Tuple<UniqueIdentifier, string, string>(target.ToLatest(),valueReq.ValueName, configuration.Key);

                        object value;
                        if (valueIndex.TryGetValue(key, out value))
                        {
                            values.Add(GetColumnHeading(configuration.Key, valueReq.ValueName), value);
                        }
                    }
                }
                yield return new PrimitiveRow(target, values);

            }
        }

        public static IEnumerable<string> GetPrimitiveColumns(ViewDefinition viewDefinition)
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

        public static IEnumerable<string> GetPortfolioColumns(ViewDefinition viewDefinition)
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

        private static IEnumerable<TreeNode> GetNodes(Portfolio portfolio, RemoteSecuritySource remoteSecuritySource)
        {
            return  GetNodesInner(portfolio.Root, remoteSecuritySource).ToList();
        }

        private static IEnumerable<TreeNode> GetNodesInner(PortfolioNode node, RemoteSecuritySource remoteSecuritySource)
        {
            yield return  new TreeNode(UniqueIdentifier.Parse(node.Identifier), node.Name);
            foreach (var position in node.Positions)
            {
                var securityNames = remoteSecuritySource.GetSecurities(position.SecurityKey.Identifiers).Select(s => s.Name).Distinct().ToList();
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

        public class Row
        {
            private readonly string _positionName;
            private readonly UniqueIdentifier _id;
            private readonly Dictionary<string, object> _columns;

            public Row(UniqueIdentifier id, string positionName, Dictionary<string, object> columns)
            {
                _id = id;
                _positionName = positionName;
                _columns = columns;
            }

            public string PositionName
            {
                get
                {
                    return _positionName;
                }
            }

            public Dictionary<string, object> Columns
            {
                get
                {
                    return _columns;
                }
            }

            public object this[String key]
            {
                get { return _columns[key]; }
            }
        }

        public class PrimitiveRow
        {
            private readonly UniqueIdentifier _targetId;
            private readonly Dictionary<string, object> _columns;

            public PrimitiveRow(UniqueIdentifier targetId, Dictionary<string, object> columns)
            {
                _targetId = targetId;
                _columns = columns;
            }

            public string TargetName
            {
                get { return _targetId.ToString(); }//This is what the WebUI does
            }

            public object this[string key]
            {
                get { return _columns.ContainsKey(key) ? _columns[key] : null; }
            }
        }

        private static IEnumerable<Row> BuildPortfolioRows(ViewDefinition viewDefinition, Portfolio portfolio, Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex, RemoteSecuritySource remoteSecuritySource)
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


                yield return new Row(position.Identifier, position.Name, values);
            }
        }
    }
}
