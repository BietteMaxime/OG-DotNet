using System.Windows.Controls;
using System.Windows.Input;

namespace OGDotNet_Analytics.View.CellTemplates
{
    public partial class DoubleLabelledMatrixCell : UserControl
    {
        public DoubleLabelledMatrixCell()
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
