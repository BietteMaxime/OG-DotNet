//-----------------------------------------------------------------------
// <copyright file="ComputationResultsTableView.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.AnalyticsViewer.View.CellTemplates;
using OGDotNet.AnalyticsViewer.ViewModel;
using OGDotNet.SecurityViewer.View;
using OGDotNet.WPFUtils;

namespace OGDotNet.AnalyticsViewer.View
{
    /// <summary>
    /// Interaction logic for ComputationResultsTableview.xaml
    /// </summary>
    public partial class ComputationResultsTableView
    {
        public ComputationResultsTableView()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var portfolioView = (GridView)portfolioTable.View;
            var primitivesView = (GridView)primitivesTable.View;

            TrimColumns(portfolioView.Columns, 1);
            TrimColumns(primitivesView.Columns, 1);

            if (e.OldValue is ComputationResultsTables)
            {
                ((ComputationResultsTables) e.OldValue).ResultReceived -= resultsTables_ResultReceived;
            }
            if (DataContext is ComputationResultsTables)
            {
                var resultsTables = (ComputationResultsTables)DataContext;
                resultsTables.ResultReceived += resultsTables_ResultReceived;
                var primitiveColumns = resultsTables.PrimitiveColumns;
                var portfolioColumns = resultsTables.PortfolioColumns;

                portfolioView.Columns.AddRange(portfolioColumns.Select(BuildColumn));
                primitivesView.Columns.AddRange(primitiveColumns.Select(BuildColumn));
            }
        }

        private void resultsTables_ResultReceived(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action) delegate
                                           {
                                               if (DataContext is ComputationResultsTables)
                                               {
                                                   var resultsTables = (ComputationResultsTables)DataContext;
                                                   resultsTables.ResultReceived -= resultsTables_ResultReceived;
                                               }

                                               //Auto resize this columns just this once
                                               var portfolioView = (GridView)portfolioTable.View;
                                               var primitivesView = (GridView)primitivesTable.View;
                                               foreach (var column in portfolioView.Columns.Concat(primitivesView.Columns))
                                               {
                                                   if (double.IsNaN(column.Width))
                                                   {
                                                       column.Width = column.ActualWidth;
                                                       column.Width = double.NaN;
                                                   }
                                               }
                                           });
        }

        private static GridViewColumn BuildColumn(ColumnHeader column)
        {
            var gridViewColumn = new GridViewColumn
                                    {
                                        Width = Double.NaN,
                                        Header = column,
                                        HeaderTemplate = TemplateTypeSelector.BuildTemplate(column, typeof(ColumnHeader))
                                    };
            gridViewColumn.CellTemplateSelector = new DirectlyIndexedCellTemplateSelector<ColumnHeader>(column, gridViewColumn);
            return gridViewColumn;
        }

        private static void TrimColumns(GridViewColumnCollection gridViewColumnCollection, int length)
        {
            while (gridViewColumnCollection.Count > length)
            {
                gridViewColumnCollection.RemoveAt(length);
            }
        }

        private void portfolioTable_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = (PortfolioRow)portfolioTable.SelectedItem;
            if (selectedItem != null && selectedItem.Security != null)
            {
                SecurityTimeSeriesWindow.ShowDialog(new[] { selectedItem.Security }, GetWindow());
            }
        }
        private Window GetWindow()
        {
            DependencyObject obj = this;
            do
            {
                obj = LogicalTreeHelper.GetParent(obj);
            }
            while (!typeof(Window).IsAssignableFrom(obj.GetType()));

            return (Window)obj;
        }
    }
}
