//-----------------------------------------------------------------------
// <copyright file="MultipleCurrencyAmountCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.Mappedtypes.financial.analytics;
using OGDotNet.Mappedtypes.Util.Money;
using Currency = OGDotNet.Mappedtypes.Util.Money.Currency;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// Interaction logic for MultipleCurrencyAmountCell.xaml
    /// </summary>
    public partial class MultipleCurrencyAmountCell : UserControl
    {
        public MultipleCurrencyAmountCell()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MultipleCurrencyAmount)
            {
                matrixCell.DataContext = GetMatrix((MultipleCurrencyAmount) DataContext);
            }
        }

        private static LabelledMatrix1D<Currency> GetMatrix(MultipleCurrencyAmount dataContext)
        {
            var keys = dataContext.Amounts.Keys.ToList();
            return new LabelledMatrix1D<Currency>(keys, keys.Select(c => c.ISOCode).Cast<object>().ToList(), keys.Select(k => dataContext.Amounts[k].Amount).ToList());
        }
    }
}
