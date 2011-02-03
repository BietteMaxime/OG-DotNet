using System.Windows;
using System.Windows.Controls;

namespace OGDotNet_AnalyticsViewer.View.CellTemplates
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

        public CellTemplateSelector CellTemplateSelector
        {
            get { return (CellTemplateSelector) GetValue(CellTemplateSelectorProperty); }
            set{SetValue(CellTemplateSelectorProperty, value);}
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
