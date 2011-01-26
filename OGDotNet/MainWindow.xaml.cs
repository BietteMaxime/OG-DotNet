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



        public MainWindow()
        {

            

            InitializeComponent();
            grid.Items.Clear();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private IList<SecurityDocument> GetThem(string name, string type, long cancellationToken)
        {
            var remoteSecuritySource = new RemoteSecurityMasterResource("http://localhost:8080/jax/");
            var securityMasters = remoteSecuritySource.GetSecurityMasters();
            CancelIfCancelled(cancellationToken);
            foreach (var securityMaster in securityMasters)
            {
                CancelIfCancelled(cancellationToken);
                return securityMaster.Search(name, type).ToList();
            }
            throw new ArgumentException();
        }

        private void CancelIfCancelled(long cancellationToken)
        {
            if (cancellationToken != Interlocked.Read(ref cancellationToken))
            {
                throw new OperationCanceledException();
            }
        }

        private void Update()
        {
            var value = new object();
            var previous = _cancellationToken+1;
            while (Interlocked.Increment(ref _cancellationToken) != previous)
            {
                previous = _cancellationToken+1;
            }

            string type = typeBox.Text;
            string name = nameBox.Text;
            BackgroundWorker worker = new BackgroundWorker();
            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 var securityDocuments = GetThem(name, type, previous).Select(s => s.Security).ToList();
                                                 CancelIfCancelled(previous);
                                                 Dispatcher.BeginInvoke((Action) (() =>
                                                                            {
                                                                                CancelIfCancelled(previous);
                                                                                grid.DataContext = securityDocuments;
                                                                            }));
                                             });
            
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
    }
}
