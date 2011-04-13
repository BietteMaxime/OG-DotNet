//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OGDotNet.Model.Context;
using OGDotNet.WPFUtils;
using OGDotNet.Model.Resources;
using OGDotNet.AnalyticsViewer.ViewModel;
using OGDotNet.WPFUtils.Windsor;

namespace OGDotNet.AnalyticsViewer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : OGDotNetWindow
    {
        private static readonly Properties.Settings Settings = Properties.Settings.Default;
        private ISecuritySource _remoteSecuritySource;
        private RemoteViewProcessor _remoteViewProcessor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs args)
        {
            try
            {
                Title = string.Format("OGDotNet ({0})", OGContext.RootUri);


                _remoteViewProcessor = OGContext.ViewProcessor;
                var viewNames = _remoteViewProcessor.ViewNames;
                _remoteSecuritySource = OGContext.SecuritySource;
                viewSelector.DataContext = viewNames;

                WindowLocationPersister.InitAndPersistPosition(this, Settings);

                var viewToSelect = viewNames.Where(v => Settings.PreviousViewName == v).FirstOrDefault();
                viewSelector.SelectedItem = viewToSelect;
            }
            catch (WebException e)
            {
                MessageBox.Show(e.ToString(), "Failed to connect to server");
                Close();
            }
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

        private void RefreshMyData(string viewName, CancellationToken cancellationToken)
        {
            try
            {
                Invoke(delegate { resultsTableView.DataContext = null; }, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                var remoteViewResource = _remoteViewProcessor.GetView(viewName);

                cancellationToken.ThrowIfCancellationRequested();
                SetStatus("Initializing view...");
                remoteViewResource.Init(cancellationToken);

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
                        SetStatus("Getting first result");
                        foreach (var results in client.GetResults(cancellationToken))
                        {
                            resultsTable.Update(results, cancellationToken);
                            SetStatus(string.Format("calculated {0} in {1} ms. ({2})", results.ValuationTime, (results.ResultTimestamp.ToDateTime() - results.ValuationTime.ToDateTime()).TotalMilliseconds, ++count));
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

        private void SetStatus(string msg)
        {
            Dispatcher.Invoke((Action)(() => { statusText.Text = msg; }));
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.Save();
            viewSelector.SelectedItem = null;
        }


    }
}

