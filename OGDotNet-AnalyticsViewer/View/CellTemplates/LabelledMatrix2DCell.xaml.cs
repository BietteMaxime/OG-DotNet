// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabelledMatrix2DCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using OpenGamma.Financial.Analytics;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// Interaction logic for LabelledMatrix2DCell.xaml
    /// </summary>
    public partial class LabelledMatrix2DCell : UserControl
    {
        public LabelledMatrix2DCell()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            grid.Columns.Clear();

            if (DataContext is IEnumerable<LabelledMatrixEntry2D>)
            {
                var entrys = (IEnumerable<LabelledMatrixEntry2D>) DataContext;
                var lookup = entrys.ToLookup(entry => entry.YLabel).ToDictionary(g => g.Key, g => g.ToDictionary(entry => entry.XLabel, entry => entry.Value));
                grid.ItemsSource = lookup.OrderBy(k => k.Key).ToList();

                var xs = lookup.SelectMany(kvp => kvp.Value.Keys).Distinct().OrderBy(x => x).ToList();

                grid.Columns.Add(new DataGridTextColumn
                                     {
                                         Header = "Y \\ X", 
                                         Binding =
                                                         new Binding(".Key")
                                                         {
                                                             Mode = BindingMode.OneWay
                                                         }
                                     });
                foreach (var x in xs)
                {
                    grid.Columns.Add(new DataGridTextColumn
                                         {
                        Header = x, 
                        Binding =
                                        new Binding(".Key")
                                        {
                                            Mode = BindingMode.OneWay, 
                                            Path = GetPath(x), 
                                            StringFormat = "{0:N2}"
                                        }
                    });
                }

                 summaryBlock.Visibility = Visibility.Visible;
            }
            else
            {
                grid.ItemsSource = null;
                summaryBlock.Visibility = Visibility.Hidden;
            }
        }

        private static PropertyPath GetPath(object x)
        {
            return new PropertyPath(".Value[(0)]", x);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
        }
    }
}
