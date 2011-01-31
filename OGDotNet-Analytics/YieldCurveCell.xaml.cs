﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet_Analytics.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet_Analytics.Mappedtypes.math.curve;

namespace OGDotNet_Analytics
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class YieldCurveCell : UserControl
    {
        public YieldCurveCell()
        {
            InitializeComponent();
        }


        private YieldCurve YieldCurve
        {
            get { return (DataContext as YieldCurve); }
        }
        
        protected InterpolatedDoublesCurve Curve
        {
            get { return YieldCurve.Curve; }
        }


        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGraph();//TODO use data binding?
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateGraph();//TODO use data binding?
        }


        private void UpdateGraph()
        {
            if (YieldCurve != null)
            {
                var doubleMinX = Curve.XData.Min();
                var doubleMaxX = Curve.XData.Max();
                double xScale = ActualWidth/(doubleMaxX - doubleMinX);

                var doubleMinY = Curve.YData.Min();
                var doubleMaxY = Curve.YData.Max();
                double yScale = ActualHeight / (doubleMaxY - doubleMinY);

                myLine.Points.Clear();
                foreach (var tuple in    Curve.Data)
                {
                    var x = (tuple.Item1 - doubleMinX) * xScale;
                    var y = ActualHeight - ((tuple.Item2 - doubleMinY) * yScale);
                    myLine.Points.Add(new Point(x, y));
                }
            }
        }

    }
}
