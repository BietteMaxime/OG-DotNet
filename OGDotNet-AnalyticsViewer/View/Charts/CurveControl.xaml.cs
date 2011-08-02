//-----------------------------------------------------------------------
// <copyright file="CurveControl.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGDotNet.Mappedtypes.Math.Curve;

namespace OGDotNet.AnalyticsViewer.View.Charts
{
    /// <summary>
    /// Interaction logic for CurveControl.xaml
    /// </summary>
    public partial class CurveControl : UserControl
    {
        public class NearestPointEventArgs : EventArgs
        {
            private readonly int _pointIndex;

            public NearestPointEventArgs(int pointIndex)
            {
                _pointIndex = pointIndex;
            }

            public int PointIndex
            {
                get { return _pointIndex; }
            }
        }

        public event EventHandler<NearestPointEventArgs> PointClicked;
        public event EventHandler<NearestPointEventArgs> NearestPointChanged;

        public CurveControl()
        {
            InitializeComponent();
        }

        public double? YMax { get; set; }
        public double? YMin { get; set; }
        public double StrokeThickness
        {
            get { return myLine.StrokeThickness; }
            set { myLine.StrokeThickness = value; }
        }

        public bool ShowName
        {
            get { return nameGroup.Height == 0.0; }
            set { nameGroup.Height = value ? double.NaN : 0.0; }
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

                var doubleMinY = YMin.GetValueOrDefault(Math.Min(Curve.YData.Min(), 0));
                var doubleMaxY = YMax.GetValueOrDefault(Curve.YData.Max());
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

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            NearestPointEventArgs nearestPointEventArgs = GetNearestPoint(e);
            InvokePointClicked(nearestPointEventArgs);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            NearestPointEventArgs nearestPointEventArgs = GetNearestPoint(e);
            pointMarker.Visibility = Visibility.Visible;
            var point = myLine.Points[nearestPointEventArgs.PointIndex];
            Canvas.SetTop(pointMarker, point.Y - pointMarker.Height / 2);
            Canvas.SetLeft(pointMarker, point.X - pointMarker.Width / 2);
            InvokeNearestPointChanged(nearestPointEventArgs);
        }

        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            pointMarker.Visibility = Visibility.Hidden;
        }

        private NearestPointEventArgs GetNearestPoint(MouseEventArgs e)
        {
            var mouse = e.GetPosition(myLine);
            var item1 = myLine.Points.Select((p, i) => Tuple.Create(i, Point.Subtract(p, mouse).LengthSquared)).OrderBy(t => t.Item2).First().Item1;
            return new NearestPointEventArgs(item1);
        }

        private void InvokePointClicked(NearestPointEventArgs e)
        {
            EventHandler<NearestPointEventArgs> handler = PointClicked;
            if (handler != null) handler(this, e);
        }

        public void InvokeNearestPointChanged(NearestPointEventArgs e)
        {
            EventHandler<NearestPointEventArgs> handler = NearestPointChanged;
            if (handler != null) handler(this, e);
        }
    }
}
