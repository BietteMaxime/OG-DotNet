using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace OGDotNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        long _cancellationToken = long.MinValue;

        static readonly RemoteSecurityMasterResource RemoteSecuritySource = new RemoteSecurityMasterResource("http://localhost:8080/jax/");
        static readonly RemoteSecurityMaster SecurityMaster = RemoteSecuritySource.GetSecurityMaster("0");
            


        public MainWindow()
        {

            

            InitializeComponent();
            grid.Items.Clear();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private SearchResults<SecurityDocument> GetThem(string name, string type, int currentPage, long cancellationToken)
        {
            CancelIfCancelled(cancellationToken);
            return SecurityMaster.Search(name, type, currentPage);
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

            BackgroundWorker worker = new BackgroundWorker();
            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 try
                                                 {
                                                     var results = GetThem(name, type, currentPage, token);
                                                     CancelIfCancelled(token);
                                                     Dispatcher.Invoke((Action) (() =>
                                                                                          {
                                                                                              CancelIfCancelled(token);
                                                                                              grid.DataContext = results.Documents.Select(s => s.Security).ToList(); //TODO
                                                                                              grid.SelectedIndex = 0;
                                                                                              pageCountLabel.DataContext = results.Paging;
                                                                                              currentPageLabel.DataContext = results.Paging;
                                                                                          }));
                                                 }
                                                 catch (OperationCanceledException e)
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
            if (grid.SelectedItem != null)
            {
                var uniqueIdentifier = (((ManageableSecurity) grid.SelectedItem)).UniqueId;
                var security = SecurityMaster.GetSecurity(uniqueIdentifier);
                MessageBox.Show(security.Name);
            }
        }

        private void grid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            detailsGrid.DataContext = grid.SelectedItem;
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
