using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OGDotNet_Analytics
{
    /// <summary>
    /// Interaction logic for VolatilitySurfaceCell.xaml
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {
        private bool _haveInitedData;
        public VolatilitySurfaceCell()
        {
            InitializeComponent();
        }

        private void InitData()
        {
            if (_haveInitedData) return;
            _haveInitedData = true;

            var data = (VolatilitySurfaceData)DataContext;

            var view = (GridView)detailsList.View;

            foreach (var x in data.Xs)
            {
                view.Columns.Add(new GridViewColumn
                                     {
                                         Width = Double.NaN,
                                         Header = x,
                                         DisplayMemberBinding = new Binding(string.Format("[{0}]", x))
                                     });
            }

            
            var rows = new List<Dictionary<string, object>>();

            foreach (var y in data.Ys)
            {
                var row = new Dictionary<string, object>();
                row["Length"] = y.ToString();

                foreach (var x in data.Xs)
                {
                    row[x.ToString()] = data[x, y];
                }
                rows.Add(row);
            }

            detailsList.ItemsSource = rows;
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InitData();
            popup.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            popup.IsOpen = false;
        }

    }
}
