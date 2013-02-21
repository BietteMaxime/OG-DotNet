// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YieldCurveCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Controls;
using System.Windows.Input;

using OGDotNet.AnalyticsViewer.View.Charts;

using OpenGamma.Analytics.Financial.Model.InterestRate.Curve;
using OpenGamma.Analytics.Math.Curve;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// Interaction logic for YieldCurveCell.xaml
    /// </summary>
    public partial class YieldCurveCell : UserControl
    {
        public YieldCurveCell()
        {
            InitializeComponent();
        }

        private YieldCurve YieldCurve
        {
            get { return DataContext as YieldCurve; }
        }

        private Curve Curve
        {
            get { return YieldCurve.Curve; }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled && ! Curve.IsVirtual)
            {
                detailsPopup.DataContext = Curve.GetData();
                detailsPopup.IsOpen = true;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
        }

        private void curveControl_NearestPointChanged(object sender, CurveControl.NearestPointEventArgs e)
        {
            itemsView.SelectedIndex = e.PointIndex;
        }
    }
}
