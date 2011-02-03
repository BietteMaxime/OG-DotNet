using System.Windows;
using System.Windows.Controls;

namespace OGDotNet_Analytics.View.CellTemplates
{
    /// <summary>
    /// Interaction logic for NullCell.xaml
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
