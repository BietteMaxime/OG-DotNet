//-----------------------------------------------------------------------
// <copyright file="GenericVolatilitySurfaceCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                IEnumerable<LabelledMatrixEntry> innerValue = GetInner(volatilitySurfaceData).ToList();
                matrixCell.DataContext = innerValue;
            }
            else
            {
                matrixCell.DataContext = null;
            }
        }

        private static IEnumerable<LabelledMatrixEntry> GetInner(VolatilitySurfaceData volatilitySurfaceData)
        {
            return GenericUtils.Call<IEnumerable<LabelledMatrixEntry>>(typeof(GenericVolatilitySurfaceCell), "GetInner", typeof(VolatilitySurfaceData<,>), volatilitySurfaceData);
        }

        public static IEnumerable<LabelledMatrixEntry> GetInner<TX, TY>(VolatilitySurfaceData<TX, TY> volatilitySurfaceData)
        {
            foreach (var x in volatilitySurfaceData.Xs)
            {
                foreach (var y in volatilitySurfaceData.Ys)
                {
                    yield return new LabelledMatrixEntry(Tuple.Create(x, y), volatilitySurfaceData[x, y]);
                }
            }
        }
    }
}
