//-----------------------------------------------------------------------
// <copyright file="NullCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// This class exists so that <see cref="CellTemplateSelector"/> can defer decisions if it's only seen nulls so far
    /// </summary>
    internal partial class NullCell : UserControl
    {
        public NullCell()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CellTemplateSelectorProperty = DependencyProperty.RegisterAttached("CellTemplateSelector", typeof(CellTemplateSelector), typeof(NullCell));

        private CellTemplateSelector CellTemplateSelector
        {
            get { return (CellTemplateSelector) GetValue(CellTemplateSelectorProperty); }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && CellTemplateSelector != null)
            {
                CellTemplateSelector.UpdateNullTemplate(DataContext);
            }
        }

    }
}
