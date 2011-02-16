using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.Mappedtypes.math.curve;

namespace OGDotNet.AnalyticsViewer.View.Charts
{
    /// <summary>
    /// Interaction logic for CurveControl.xaml
    /// </summary>
    public partial class CurveControl : UserControl
    {
        public CurveControl()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGraph();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            if (Curve != null)
            {
                if (Curve.IsVirtual)
                {//We can't display this
                    IsEnabled = false;
                    ShowDisabled();
                    canvas.Visibility = Visibility.Visible;
                    return;
                }

                var doubleMinX = Curve.XData.Min();
                var doubleMaxX = Curve.XData.Max();
                double xScale = canvas.ActualWidth / (doubleMaxX - doubleMinX);

                var doubleMinY = Math.Min(Curve.YData.Min(), 0);
                var doubleMaxY = Curve.YData.Max();
                double yScale = canvas.ActualHeight / (doubleMaxY - doubleMinY);

                myLine.Points.Clear();
                foreach (var tuple in Curve.GetData())
                {
                    var x = (tuple.Item1 - doubleMinX) * xScale;
                    var y = canvas.ActualHeight - ((tuple.Item2 - doubleMinY) * yScale);
                    myLine.Points.Add(new Point(x, y));
                }

                xAxis.X1 = 0;
                xAxis.X2 = canvas.ActualWidth;

                xAxis.Y1 = canvas.ActualHeight - ((-doubleMinY) * yScale);
                xAxis.Y2 = xAxis.Y1;

                yAxis.X1 = 0;
                yAxis.X2 = 0;
                yAxis.Y1 = canvas.ActualHeight;
                yAxis.Y2 = 0;
                IsEnabled = true;
                canvas.Visibility = Visibility.Visible;
            }
            else
            {
                canvas.Visibility = Visibility.Hidden;
            }
            nameGroup.Visibility = canvas.Visibility;
        }

        private Curve Curve
        {
            get { return DataContext as Curve; }
        }

        private void ShowDisabled()
        {
            xAxis.X1 = 0;
            xAxis.X2 = canvas.ActualWidth;

            xAxis.Y1 = canvas.ActualHeight;
            xAxis.Y2 = 0;

            yAxis.X1 = 0;
            yAxis.X2 = canvas.ActualWidth;
            yAxis.Y1 = 0;
            yAxis.Y2 = canvas.ActualHeight;

            myLine.Points.Clear();
        }
    }
}
