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
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model.Resources;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class ComputationResultsTables : PortfolioResultsTableBase<PortfolioRow>
    {
        public event EventHandler ResultReceived;

        private readonly ViewDefinition _viewDefinition;
        private readonly ICompiledViewDefinition _compiledViewDefinition;
        private readonly List<ColumnHeader> _portfolioColumns;
        private readonly List<ColumnHeader> _primitiveColumns;

        private readonly List<PrimitiveRow> _primitiveRows;

        public ComputationResultsTables(ISecuritySource remoteSecuritySource, ICompiledViewDefinition compiledViewDefinition)
            : base(remoteSecuritySource, compiledViewDefinition.Portfolio)
        {
            _viewDefinition = compiledViewDefinition.ViewDefinition;
            _compiledViewDefinition = compiledViewDefinition;
            _portfolioColumns = GetPortfolioColumns(_viewDefinition, _compiledViewDefinition).ToList();
            _primitiveColumns = GetPrimitiveColumns(_viewDefinition, _compiledViewDefinition).ToList();

            _primitiveRows = BuildPrimitiveRows().ToList();
        }

        public void Update(CycleCompletedArgs results)
        {
            var delta = results.FullResult ?? (IViewResultModel) results.DeltaResult;

            var indexedResults = delta.AllResults.ToLookup(v => v.ComputedValue.Specification.TargetSpecification.Uid);
            
            UpdatePortfolioRows(PortfolioRows, indexedResults);
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

        public List<PrimitiveRow> PrimitiveRows
        {
            get { return _primitiveRows; }
        }

        private static IEnumerable<ColumnHeader> GetPrimitiveColumns(ViewDefinition viewDefinition, ICompiledViewDefinition compiledViewDefinition)
        {
            var columns = new HashSet<ColumnHeader>();
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                Dictionary<ValueSpecification, HashSet<ValueRequirement>> specs = compiledViewDefinition.CompiledCalculationConfigurations[configuration.Key].TerminalOutputSpecifications;
                
                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.TargetSpecification.Type == ComputationTargetType.Primitive))
                {
                    //TODO less slow
                    if (Satisfies(specs, Tuple.Create(req.ValueName, req.Constraints)))
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
                Dictionary<ValueSpecification, HashSet<ValueRequirement>> specs = compiledViewDefinition.CompiledCalculationConfigurations[configuration.Key].TerminalOutputSpecifications;

                foreach (var secType in configuration.Value.PortfolioRequirementsBySecurityType)
                {
                    foreach (var req in secType.Value)
                    {
                        if (Satisfies(specs, req))
                        {
                            columns.Add(new ColumnHeader(configuration.Key, req.Item1, req.Item2));
                        }
                    }
                }
            }
            return columns;
        }

        private static bool Satisfies(Dictionary<ValueSpecification, HashSet<ValueRequirement>> specs, Tuple<string, ValueProperties> req)
        {
            return specs.Where(s => s.Key.ValueName.Equals(req.Item1) && s.Value.Any(r => r.Constraints.IsSatisfiedBy(req.Item2))).Any();
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

        protected override PortfolioRow WrapPortfolioRow(PortfolioViewTreeNode viewTreeNode)
        {
            return new PortfolioRow(viewTreeNode);
        }
    }
}
