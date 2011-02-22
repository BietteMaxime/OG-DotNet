using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Util.Db;
using OGDotNet.Model.Resources;
using OGDotNet.WPFUtils.Windsor;

namespace OGDotNet.SecurityViewer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SecurityWindow : OGDotNetWindow
    {
        long _cancellationToken = long.MinValue;


        public SecurityWindow()
        {
            InitializeComponent();
            itemGrid.Items.Clear();
        }


        private RemoteSecurityMaster SecurityMaster
        {
            get { return OGContext.SecurityMaster; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void CancelIfCancelled(long cancellationToken)
        {
            if (cancellationToken != _cancellationToken)
            {
                throw new OperationCanceledException();
            }
        }

        private void Update()
        {
            var token = ++_cancellationToken;

            string type = typeBox.Text;
            string name = nameBox.Text;
            
            int currentPage = CurrentPage;

            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 try
                                                 {
                                                     CancelIfCancelled(token);
                                                     var results = SecurityMaster.Search(name, type, new PagingRequest(currentPage, 20));
                                                     CancelIfCancelled(token);
                                                     Dispatcher.Invoke((Action) (() =>
                                                                                          {
                                                                                              CancelIfCancelled(token);
                                                                                              itemGrid.DataContext = results.Documents.Select(s => s.Security).ToList(); //TODO
                                                                                              itemGrid.SelectedIndex = 0;
                                                                                              pageCountLabel.DataContext = results.Paging;
                                                                                              currentPageLabel.DataContext = results.Paging;
                                                                                              outerGrid.UpdateLayout();
                                                                                          }));
                                                 }
                                                 catch (OperationCanceledException)
                                                 {
                                                 }
                                             });
            
        }

        private int CurrentPage
        {
            get
            {
                int currentPage;
                if (! int.TryParse(currentPageLabel.Text, out currentPage))
                {
                    currentPage = 1;
                }
                return currentPage;
            }
            set { 
                if (value<1)
                {
                    value = 1;
                }
                currentPageLabel.Text = value.ToString();
            }
        }
        public int PageCount
        {
            get
            {
                return (int) pageCountLabel.Content; 
            }
        }

        private void typeBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
                Update();
        }

        

        private void nameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
                Update();
        }


        private void nextPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            Update();
        }
        private void lastPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = PageCount;
            Update();
        }


        private void grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (itemGrid.SelectedItem != null)
            {
                Security security = (Security) itemGrid.SelectedItem;

                Security[] securities = new Security[] {security};

                ShowSecurities(securities);
            }
        }

        private void showAll_Click(object sender, RoutedEventArgs e)
        {
            ShowSecurities(itemGrid.Items.Cast<Security>());
        }

        private void ShowSecurities(IEnumerable<Security> securities)
        {
            SecurityTimeSeriesWindow.ShowDialog(securities, this);
        }




        private void grid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            detailsGrid.DataContext = itemGrid.SelectedItem;
        }


        private void firstPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = 1;
            Update();
        }

        private void previousPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            Update();
        }
    }
}
