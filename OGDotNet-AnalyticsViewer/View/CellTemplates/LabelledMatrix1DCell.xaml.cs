//-----------------------------------------------------------------------
// <copyright file="LabelledMatrix1DCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    public partial class LabelledMatrix1DCell : UserControl
    {
        public LabelledMatrix1DCell()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            summaryBlock.Visibility = DataContext == null ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
