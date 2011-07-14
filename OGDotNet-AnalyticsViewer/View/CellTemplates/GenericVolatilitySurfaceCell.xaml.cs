//-----------------------------------------------------------------------
// <copyright file="GenericVolatilitySurfaceCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Utils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// Interaction logic for GenericVolatilitySurfaceCell.xaml
    /// </summary>
    public partial class GenericVolatilitySurfaceCell : UserControl
    {
        public GenericVolatilitySurfaceCell()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is VolatilitySurfaceData)
            {
                var volatilitySurfaceData = (VolatilitySurfaceData) DataContext;
                IEnumerable<LabelledMatrixEntry2D> innerValue = GetInner(volatilitySurfaceData).ToList();
                matrixCell.DataContext = innerValue;
            }
            else
            {
                matrixCell.DataContext = null;
            }
        }

        private static IEnumerable<LabelledMatrixEntry2D> GetInner(VolatilitySurfaceData volatilitySurfaceData)
        {
            return GenericUtils.Call<IEnumerable<LabelledMatrixEntry2D>>(typeof(GenericVolatilitySurfaceCell), "GetInner", typeof(VolatilitySurfaceData<,>), volatilitySurfaceData);
        }

        public static IEnumerable<LabelledMatrixEntry2D> GetInner<TX, TY>(VolatilitySurfaceData<TX, TY> volatilitySurfaceData)
        {
            foreach (var x in volatilitySurfaceData.Xs)
            {
                foreach (var y in volatilitySurfaceData.Ys)
                {
                    double value;
                    if (volatilitySurfaceData.TryGet(x, y, out value))
                    {
                        yield return new LabelledMatrixEntry2D(x, y, value);
                    }
                }
            }
        }
    }
}
