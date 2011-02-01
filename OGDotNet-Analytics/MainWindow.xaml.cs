using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OGDotNet_Analytics.Mappedtypes.Core.Position;
using OGDotNet_Analytics.Mappedtypes.engine;
using OGDotNet_Analytics.Mappedtypes.engine.View;
using OGDotNet_Analytics.Mappedtypes.Id;
using OGDotNet_Analytics.Model.Resources;
using OGDotNet_Analytics.View.CellTemplates;

namespace OGDotNet_Analytics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RemoteSecuritySource _remoteSecuritySource;
        private RemoteViewProcessor _remoteViewProcessor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var remoteConfig = new RemoteConfig("0", "http://127.0.0.1:8080/jax"); //devsvr-lx-2 or localhost
            
            var remoteClient = remoteConfig.UserClient;
            remoteClient.HeartbeatSender();

            _remoteViewProcessor = remoteConfig.ViewProcessor;
            var viewNames = _remoteViewProcessor.ViewNames;
            _remoteSecuritySource = remoteConfig.SecuritySource;
            viewSelector.DataContext = viewNames;

        }

        private void viewSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            pauseToggle.IsChecked = false;

            var viewName = (string) viewSelector.SelectedItem;
            if (viewName != null)
            {

                new Thread(() => RefreshMyData(viewName, _cancellationTokenSource.Token)).Start();
            }
        }


        public void RefreshMyData(string viewName, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remoteViewResource = _remoteViewProcessor.GetView(viewName);

                cancellationToken.ThrowIfCancellationRequested();
                SetStatus("Initializing view...");
                remoteViewResource.Init();

                cancellationToken.ThrowIfCancellationRequested();
                var portfolio = remoteViewResource.Portfolio;

                cancellationToken.ThrowIfCancellationRequested();
                var viewDefinition = remoteViewResource.Definition;

                Dispatcher.Invoke((Action)(() =>
                   {
                       var portfolioView = (GridView)portfolioTable.View;
                       portfolioTable.DataContext = null;

                       TrimColumns(portfolioView.Columns, 1);
                       
                       foreach (var column in GetPortfolioColumns(viewDefinition))
                       {
                           portfolioView.Columns.Add(new GridViewColumn
                                                         {
                                                             Width = Double.NaN,
                                                             Header = column,
                                                             CellTemplateSelector = new CellTemplateSelector(column)
                                                    });
                       }

                       primitivesTable.DataContext = null;
                       var primitivesView = (GridView)primitivesTable.View;

                       TrimColumns(primitivesView.Columns, 1);
                       foreach (var column in GetPrimitiveColumns(viewDefinition))
                       {
                           primitivesView.Columns.Add(new GridViewColumn
                           {
                               Width = Double.NaN,
                               Header = column,
                               CellTemplateSelector = new CellTemplateSelector(column)

                           });
                       }

                       portfolioTabItem.Visibility = viewDefinition.PortfolioIdentifier == null ? Visibility.Hidden : Visibility.Visible;
                       primitiveTabItem.Visibility = primitivesView.Columns.Count==1 ? Visibility.Hidden : Visibility.Visible;
                   }));

                int count = 0;

                SetStatus("Creating client");
                cancellationToken.ThrowIfCancellationRequested();
                using (var client = remoteViewResource.CreateClient())
                {
                    //TODO get these off the UI thread but with order
                    RoutedEventHandler pausedHandler = delegate { if (! cancellationToken.IsCancellationRequested) {client.Pause();} };
                    RoutedEventHandler unpausedHandler = delegate { if (!cancellationToken.IsCancellationRequested) { client.Start(); } };
                    pauseToggle.Checked += pausedHandler;
                    pauseToggle.Unchecked+= unpausedHandler;
                    try
                    {
                        SetStatus("Getting first result");
                        foreach (var results in client.GetResults(cancellationToken))
                        {
                            //TODO move this logic elsewhere
                            var valueIndex = Indexvalues(results);

                            cancellationToken.ThrowIfCancellationRequested();
                            var portfolioRows = BuildPortfolioRows(viewDefinition, portfolio, valueIndex).ToList();

                            cancellationToken.ThrowIfCancellationRequested();
                            var primitiveRows = BuildPrimitiveRows(viewDefinition, valueIndex).ToList();

                            cancellationToken.ThrowIfCancellationRequested();

                            Dispatcher.Invoke((Action) (() =>
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    portfolioTable.DataContext = portfolioRows;
                                    primitivesTable.DataContext = primitiveRows;
                                }
                            }));
                            SetStatus(string.Format("Awaiting next result. ({0})", ++count));
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed to retrieve data");
            }
        }

        private void SetStatus(string msg)
        {
            Dispatcher.Invoke((Action) (() => { statusText.Text = msg; }));
        }

        private static void TrimColumns(GridViewColumnCollection gridViewColumnCollection, int length)
        {
            while (gridViewColumnCollection.Count >length)
            {
                gridViewColumnCollection.RemoveAt(length);                           
            }
        }

        public class PrimitiveRow
        {
            private readonly UniqueIdentifier _targetId;
            private readonly Dictionary<string, object> _columns;

            public PrimitiveRow(UniqueIdentifier targetId, Dictionary<string, object> columns)
            {
                _targetId = targetId;
                _columns = columns;
            }

            public string TargetName
            {
                get { return _targetId.ToString(); }//This is what the WebUI does
            }

            public object this[string key]
            {
                get { return _columns.ContainsKey(key) ? _columns[key] : null; }
            }
        }
        private static IEnumerable<PrimitiveRow> BuildPrimitiveRows(ViewDefinition viewDefinition, Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex)
        {
            var targets = viewDefinition.CalculationConfigurationsByName.SelectMany(conf => conf.Value.SpecificRequirements).Select(s => s.ComputationTargetIdentifier).Distinct();
            foreach (var target in targets)
            {
                var values = new Dictionary<string, object>();
                foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var valueReq in configuration.Value.SpecificRequirements.Where(r =>r.ComputationTargetType == ComputationTargetType.PRIMITIVE && r.ComputationTargetIdentifier == target))
                    {
                        var key = new Tuple<UniqueIdentifier, string, string>(target.ToLatest(),valueReq.ValueName, configuration.Key);

                        object value;
                        if (valueIndex.TryGetValue(key, out value))
                        {
                            values.Add(GetColumnHeading(configuration.Key, valueReq.ValueName), value);
                        }
                    }
                }
                yield return new PrimitiveRow(target, values);

            }
        }

        private static IEnumerable<string> GetPrimitiveColumns(ViewDefinition viewDefinition)
        {
            var valueNames = new HashSet<string>();
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                foreach (var req in configuration.Value.SpecificRequirements.Where(r => r.ComputationTargetType == ComputationTargetType.PRIMITIVE))
                {
                    valueNames.Add(GetColumnHeading(configuration.Key, req.ValueName));
                }
            }
            return valueNames;
        }


        private static IEnumerable<string> GetPortfolioColumns(ViewDefinition viewDefinition)
        {

            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                foreach (var valuePropertiese in configuration.Value.PortfolioRequirementsBySecurityType)
                {
                    foreach (var property in valuePropertiese.Value.Properties)
                    {
                        foreach (var p in property.Value)
                        {
                            yield return GetColumnHeading(configuration.Key, p);
                        }
                    }
                }
            }
        }

        private static string GetColumnHeading(string configuration, string valueName)
        {
            return string.Format("{0}/{1}", configuration, valueName);
        }


        private static IEnumerable<TreeNode> GetNodes(Portfolio portfolio, RemoteSecuritySource remoteSecuritySource)
        {
            return  GetNodesInner(portfolio.Root, remoteSecuritySource).ToList();
        }


        private static IEnumerable<TreeNode> GetNodesInner(PortfolioNode node, RemoteSecuritySource remoteSecuritySource)
        {
            yield return  new TreeNode(UniqueIdentifier.Parse(node.Identifier), node.Name);
            foreach (var position in node.Positions)
            {
                var securityNames = remoteSecuritySource.GetSecurities(position.SecurityKey.Identifiers).Select(s => s.Name).Distinct().ToList();
                if (securityNames.Count != 1)
                {
                    throw new ArgumentException();
                }

                string securityName = securityNames[0];
                yield return new TreeNode(position.Identifier, string.Format("{0} ({1})", securityName, position.Quantity));
            }

            foreach (var portfolioNode in node.SubNodes)
            {
                foreach (var treeNode in GetNodesInner(portfolioNode, remoteSecuritySource))
                {
                    yield return treeNode;
                }
            }
        }

        private class TreeNode
        {
            private readonly UniqueIdentifier _identifier;
            private readonly string _name;

            public TreeNode(UniqueIdentifier identifier, string name)
            {
                _identifier = identifier;
                _name = name;
            }

            public UniqueIdentifier Identifier
            {
                get { return _identifier; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        private IEnumerable<Row> BuildPortfolioRows(ViewDefinition viewDefinition, Portfolio portfolio, Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex)
        {
            if (portfolio == null)
                yield break;
            foreach (var position in GetNodes(portfolio, _remoteSecuritySource))
            {
                var values = new Dictionary<string, object>();

                foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
                {
                    foreach (var req in configuration.Value.PortfolioRequirementsBySecurityType)
                    {
                        //TODO respect security type
                        foreach (var portfolioReq in req.Value.Properties["portfolioRequirement"])
                        {
                            string header = string.Format("{0}/{1}", configuration.Key, portfolioReq);
                            
                            var key = new Tuple<UniqueIdentifier, string, string>(position.Identifier.ToLatest(), portfolioReq, configuration.Key);
                            object value;
                            if (valueIndex.TryGetValue(key, out value))
                            {
                                values.Add(header, value);
                            }
                            else
                            {
                                values.Add(header, "undefined");
                            }
                        }
                    }
                }


                yield return new Row(position.Identifier, position.Name, values);
            }
        }

        private static Dictionary<Tuple<UniqueIdentifier, string, string>, object> Indexvalues(ViewComputationResultModel results)
        {
            var valueIndex = new Dictionary<Tuple<UniqueIdentifier, string, string>, object>();
            foreach (var result in results.AllResults)
            {
                
                switch (result.ComputedValue.Specification.TargetSpecification.Type)
                {
                    case ComputationTargetType.PRIMITIVE:
                    case ComputationTargetType.POSITION:
                    case ComputationTargetType.PORTFOLIO_NODE:
                        valueIndex.Add(new Tuple<UniqueIdentifier, string, string>(
                            result.ComputedValue.Specification.TargetSpecification.Uid.ToLatest(),
                            result.ComputedValue.Specification.ValueName, result.CalculationConfiguration),
                            result.ComputedValue.Value);
                        break;
                    case ComputationTargetType.TRADE:
                    case ComputationTargetType.SECURITY:
                        //TODO throw new NotImplementedException();
                        break;
                }
            }
            return valueIndex;
        }

        public class Row
        {
            private readonly string _positionName;
            private readonly UniqueIdentifier _id;
            private readonly Dictionary<string, object> _columns;

            public Row(UniqueIdentifier id, string positionName, Dictionary<string, object> columns)
            {
                _id = id;
                _positionName = positionName;
                _columns = columns;
            }

            public string PositionName
            {
                get
                {
                    return _positionName;
                }
            }

            public Dictionary<string, object> Columns
            {
                get
                {
                    return _columns;
                }
            }

            public object this[String key]
            {
                get { return _columns[key]; }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            viewSelector.SelectedItem = null;
        }

      
        }


}

