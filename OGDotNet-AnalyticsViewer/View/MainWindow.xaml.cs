//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OGDotNet.AnalyticsViewer.ViewModel;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.client;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
using OGDotNet.WPFUtils;
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
            if (OGContext == null)
            {
                try
                {
                    OGContextFactory.CreateRemoteEngineContext();
                    throw new ArgumentException("Unexpectedly succeeded this time");
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Failed to connect to remote server:\n\t{0}\nHave you updated app.config?", e.Message), "Failed to connect to server");
                }
                
                Close();
                return;
            }

            Title = string.Format("OGDotNet ({0})", OGContext.RootUri);

            _remoteViewProcessor = OGContext.ViewProcessor;
            var viewNames = _remoteViewProcessor.ViewDefinitionRepository.GetDefinitionNames();
            _remoteSecuritySource = OGContext.SecuritySource;
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
            Dispatcher.Invoke(((Action)delegate
                                            {
                                                if (!token.IsCancellationRequested)
                                                {
                                                    action();
                                                }
                                            }));
            token.ThrowIfCancellationRequested();
        }

        private void RefreshMyData(string viewName, CancellationToken cancellationToken)
        {
            try
            {
                RefreshMyDataImpl(viewName, cancellationToken);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Failed to load view data");
            }
        }

        private void RefreshMyDataImpl(string viewName, CancellationToken cancellationToken)
        {
            Invoke(delegate { resultsTableView.DataContext = null; }, cancellationToken);

            var clientLock = new object();

            lock (clientLock)
            {
                var client = OGContext.ViewProcessor.CreateClient();
                RoutedEventHandler pausedHandler = delegate
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        client.Pause();
                    }
                };
                RoutedEventHandler unpausedHandler = delegate
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        client.Resume();
                    }
                };
                //Must do this before we can throw any exceptions
                ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, delegate
                {
                    lock (clientLock)
                    {
                        client.Dispose();
                        pauseToggle.Checked -= pausedHandler;
                        pauseToggle.Unchecked -= unpausedHandler;
                    }
                }, null, int.MaxValue, true);

                //Now we can start hooking things together
                pauseToggle.Checked += pausedHandler;
                pauseToggle.Unchecked += unpausedHandler;

                ComputationResultsTables resultsTable = null;
                int count = 0;

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.ViewDefinitionCompiled +=
                    delegate(object sender, ViewDefinitionCompiledArgs args)
                        {
                            var portfolio = args.CompiledViewDefinition.Portfolio;
                            var viewDefinition = args.CompiledViewDefinition.ViewDefinition;
                            resultsTable = new ComputationResultsTables(viewDefinition, portfolio, _remoteSecuritySource);
                            Invoke(delegate
                                       {
                                           resultsTableView.DataContext = resultsTable;
                                           SetStatus(string.Format("Waiting for first cycle..."));
                                       }, cancellationToken);
                        };
                eventViewResultListener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                                                              {
                                                                  resultsTable.Update(e);
                                                                  SetStatus(GetMessage(e.FullResult ?? e.DeltaResult, ref count));
                                                              };

                eventViewResultListener.ViewDefinitionCompilationFailed +=
                    delegate(object sender, ViewDefinitionCompilationFailedArgs args)
                    {
                        Invoke(delegate
                                   {
                                       SetStatus(string.Format("Failed to compile {0} @ {1}", args.Exception, args.ValuationTime), true);
                                       resultsTableView.DataContext = null;
                                   }, cancellationToken);
                    };
                eventViewResultListener.CycleExecutionFailed +=
                    delegate(object sender, CycleExecutionFailedArgs args)
                    {
                        Invoke(delegate
                        {
                            SetStatus(string.Format("Failed to execute {0} @ {1}", args.Exception, args.ExecutionOptions.ValuationTime), true);                            
                        }, cancellationToken);
                    };

                SetStatus(string.Format("Waiting for compilation..."));
                client.SetViewResultMode(ViewResultMode.FullThenDelta);
                client.SetResultListener(eventViewResultListener);
                client.AttachToViewProcess(viewName, ExecutionOptions.RealTime);
            }
        }

        private static string GetMessage(InMemoryViewComputationResultModel results, ref int count)
        {
            return string.Format("calculated {0} in {1} ms. ({2})", results.ValuationTime, (results.ResultTimestamp.ToUniversalTime() - results.ValuationTime.ToUniversalTime()).TotalMilliseconds, ++count);
        }

        private void SetStatus(string msg, bool isError = false)
        {
            Dispatcher.Invoke((Action)(() =>
                                           {
                                               statusText.Text = msg;
                                               if (isError)
                                               {
                                                   statusBar.Background = statusBar.Background  == Brushes.Red ? Brushes.Yellow : Brushes.Red;
                                                   statusBar.Height = 90;
                                               }
                                               else
                                               {
                                                   statusBar.Background = null;
                                                   statusBar.Height = double.NaN;
                                               }
                                           }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.Save();
            viewSelector.SelectedItem = null;
        }
    }
}

