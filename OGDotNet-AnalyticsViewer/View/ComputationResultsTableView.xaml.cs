//-----------------------------------------------------------------------
// <copyright file="ComputationResultsTableView.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.SecurityViewer.View;
using OGDotNet.WPFUtils;
using OGDotNet.AnalyticsViewer.View.CellTemplates;
using OGDotNet.AnalyticsViewer.ViewModel;

namespace OGDotNet.AnalyticsViewer.View
{
    /// <summary>
    /// Interaction logic for ComputationResultsTableview.xaml
    /// </summary>
    public partial class ComputationResultsTableView
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ComputationResultsTableView()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var portfolioView = (GridView)portfolioTable.View;
            var primitivesView = (GridView)primitivesTable.View;


            TrimColumns(portfolioView.Columns, 1);
            TrimColumns(primitivesView.Columns, 1);

            if ((DataContext as ComputationResultsTables)!= null)
            {
                var resultsTables = (ComputationResultsTables) DataContext;
                var primitiveColumns = resultsTables.PrimitiveColumns;
                var portfolioColumns = resultsTables.PortfolioColumns;

                portfolioView.Columns.AddRange(portfolioColumns.Select(BuildColumn));
                primitivesView.Columns.AddRange(primitiveColumns.Select(BuildColumn));
            }
        }

        private static GridViewColumn BuildColumn(string column)
        {
            var gridViewColumn = new GridViewColumn
                                    {
                                        Width = Double.NaN,
                                        Header = column
                                    };
            gridViewColumn.CellTemplateSelector = new CellTemplateSelector(column, gridViewColumn);
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
            var selectedItem = (PortfolioRow) portfolioTable.SelectedItem;
            if (selectedItem!= null && selectedItem.Security != null)
            {
                SecurityTimeSeriesWindow.ShowDialog(new[] { selectedItem.Security }, GetWindow());
            }
        }
        private Window GetWindow()
        {
            DependencyObject obj =this;
            do
                 {
                    obj = LogicalTreeHelper.GetParent(obj);
               } while (! typeof(Window).IsAssignableFrom(obj.GetType()));

            return (Window) obj;
        }
        
    }
}
