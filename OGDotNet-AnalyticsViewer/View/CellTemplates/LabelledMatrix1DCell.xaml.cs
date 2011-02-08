using System.Windows.Controls;
using System.Windows.Input;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    public partial class LabelledMatrix1DCell : UserControl
    {
        public LabelledMatrix1DCell()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
        }
    }
}
