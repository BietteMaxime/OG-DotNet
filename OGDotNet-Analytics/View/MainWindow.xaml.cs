using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OGDotNet_Analytics.Model.Resources;
using OGDotNet_Analytics.Properties;
using OGDotNet_Analytics.View;
using OGDotNet_Analytics.View.CellTemplates;

namespace OGDotNet_Analytics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Settings Settings = Settings.Default;
        private readonly RemoteSecuritySource _remoteSecuritySource;
        private readonly RemoteViewProcessor _remoteViewProcessor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();

            Title = string.Format("OGDotNet ({0})", Settings.ServiceUri);

            var remoteConfig = new RemoteConfig(Settings.ConfigId, Settings.ServiceUri);

            var remoteClient = remoteConfig.UserClient;
            remoteClient.HeartbeatSender();

            _remoteViewProcessor = remoteConfig.ViewProcessor;
            var viewNames = _remoteViewProcessor.ViewNames;
            _remoteSecuritySource = remoteConfig.SecuritySource;
            viewSelector.DataContext = viewNames;

            WindowLocationPersister.InitAndPersistPosition(this, Settings);

            var viewToSelect = viewNames.Where(v => Settings.PreviousViewName == v).FirstOrDefault();
            viewSelector.SelectedItem = viewToSelect;
        }

        private void viewSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            pauseToggle.IsChecked = false;

            var viewName = (string)viewSelector.SelectedItem;

            Settings.PreviousViewName = viewName;

            if (viewName != null)
            {
                new Thread(() => RefreshMyData(viewName, _cancellationTokenSource.Token)) { Name = "MainWindow.RefreshMyData thread" }.Start();
            }
        }

        private void Invoke(Action action, CancellationToken token)
        {
            Dispatcher.Invoke(((Action)delegate { if (!token.IsCancellationRequested) { action(); } }));
            token.ThrowIfCancellationRequested();
        }

        public void RefreshMyData(string viewName, CancellationToken cancellationToken)
        {
            try
            {
                Invoke(delegate { resultsTableView.DataContext = null; }, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                var remoteViewResource = _remoteViewProcessor.GetView(viewName);

                cancellationToken.ThrowIfCancellationRequested();
                SetStatus("Initializing view...");
                remoteViewResource.Init();

                cancellationToken.ThrowIfCancellationRequested();
                var portfolio = remoteViewResource.Portfolio;

                cancellationToken.ThrowIfCancellationRequested();
                var viewDefinition = remoteViewResource.Definition;

                
                var resultsTable = new ComputationResultsTables(viewDefinition, portfolio, _remoteSecuritySource);
                Invoke(delegate { resultsTableView.DataContext = resultsTable; }, cancellationToken);
                
                

                int count = 0;

                SetStatus("Creating client");
                using (var client = remoteViewResource.CreateClient())
                {
                    //TODO get these off the UI thread but with order
                    RoutedEventHandler pausedHandler = delegate { if (!cancellationToken.IsCancellationRequested) { client.Pause(); } };
                    RoutedEventHandler unpausedHandler = delegate { if (!cancellationToken.IsCancellationRequested) { client.Start(); } };
                    pauseToggle.Checked += pausedHandler;
                    pauseToggle.Unchecked += unpausedHandler;
                    try
                    {
                        DateTime previousTime = DateTime.Now;
                        SetStatus("Getting first result");
                        foreach (var results in client.GetResults(cancellationToken))
                        {
                            resultsTable.Update(results, cancellationToken);
                            SetStatus(string.Format("calculated {0} in {1} ms. ({2})", results.ValuationTime, (DateTime.Now - previousTime).TotalMilliseconds, ++count));
                            previousTime = DateTime.Now;
                        }
                    }
                    finally
                    {
                        pauseToggle.Checked -= pausedHandler;
                        pauseToggle.Unchecked -= unpausedHandler;
                    }
                }
            }
            catch (OperationCanceledException)//TODO don't use exceptions here
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed to retrieve data");
            }
        }

        private static GridViewColumn BuildColumn(string column)
        {
            return new GridViewColumn
                       {
                           Width = Double.NaN,
                           Header = column,
                           CellTemplateSelector = new CellTemplateSelector(column)
                       };
        }

        private void SetStatus(string msg)
        {
            Dispatcher.Invoke((Action)(() => { statusText.Text = msg; }));
        }

        private static void TrimColumns(GridViewColumnCollection gridViewColumnCollection, int length)
        {
            while (gridViewColumnCollection.Count > length)
            {
                gridViewColumnCollection.RemoveAt(length);
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.Save();
            viewSelector.SelectedItem = null;
        }
    }
}

