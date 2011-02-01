using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet;
using OGDotNet_Analytics.Mappedtypes.LiveData;
using OGDotNet_Analytics.Mappedtypes.math.curve;

namespace OGDotNet_Analytics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _counter;
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


                cancellationToken.ThrowIfCancellationRequested();
                using (var client = remoteViewResource.CreateClient())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    client.Start();

                    
                    SetStatus("Waiting for results...");
                    while (! client.ResultAvailable)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    
                    cancellationToken.ThrowIfCancellationRequested();
                    using (var deltaStream = client.StartDeltaStream())
                    {
                        //NOTE: by starting the delta stream first I believe I am ok
                        SetStatus("Getting first results...");
                        cancellationToken.ThrowIfCancellationRequested();
                        ViewComputationResultModel results = client.LatestResult;


                        int count = 1;

                        while (! cancellationToken.IsCancellationRequested)
                        {    
                            var valueIndex = Indexvalues(results);

                            cancellationToken.ThrowIfCancellationRequested();
                            var portfolioRows = BuildPortfolioRows(viewDefinition, portfolio, valueIndex).ToList();

                            cancellationToken.ThrowIfCancellationRequested();
                            var primitiveRows = BuildPrimitiveRows(viewDefinition, valueIndex).ToList();
                            
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            bool paused = false;
                            Dispatcher.Invoke((Action) (() =>
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                portfolioTable.DataContext = portfolioRows;
                                                                
                                primitivesTable.DataContext = primitiveRows;
                                paused = pauseToggle.IsChecked.GetValueOrDefault(false);
                            }));


                            cancellationToken.ThrowIfCancellationRequested();
                            if (paused)
                            {
                                SetStatus("Pausing...");
                                cancellationToken.ThrowIfCancellationRequested();
                                client.Pause();
                                SetStatus("Paused");
                                WaitForUncheck(pauseToggle, cancellationToken);
                                cancellationToken.ThrowIfCancellationRequested();
                                client.Start();
                            }
                            cancellationToken.ThrowIfCancellationRequested();

                            SetStatus(string.Format("Waiting for next result {0}... ", ++count));
                            var delta = deltaStream.GetNext(cancellationToken);
                            results = results.ApplyDelta(delta);
                        }

                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed to retrieve data");
            }
        }

        private void WaitForUncheck(ToggleButton toggleButton, CancellationToken cancellationToken)
        {
            using (var autoResetEvent = new AutoResetEvent(false))
            using (cancellationToken.Register(() => autoResetEvent.Set()))
            {
                bool done = false;

                RoutedEventHandler clickHandler = delegate { autoResetEvent.Set(); };
                toggleButton.Click += clickHandler;
                try
                {
                    do
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            done = !toggleButton.IsChecked.GetValueOrDefault(false);
                        }));
                        if (! done) autoResetEvent.WaitOne();
                    } while (! done);
                    
                }
                finally
                {
                    toggleButton.Click -= clickHandler;
                }
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


        private IEnumerable<TreeNode> GetNodes(Portfolio portfolio, RemoteSecuritySource remoteSecuritySource)
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

        private static IEnumerable<Position> GetPositions(PortfolioNode portfolio)
        {
            foreach (var position in portfolio.Positions)
            {
                yield return position;
            }
            foreach (var portfolioNode in portfolio.SubNodes)
            {
                foreach (var position in GetPositions(portfolioNode))
                {
                    yield return position;
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

    public class Portfolio
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public PortfolioNode Root { get; set; }
    }

    public class PortfolioNode
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public IList<PortfolioNode> SubNodes { get; set; }
        public IList<Position> Positions { get; set; }
    }

    public class PositionBuilder : BuilderBase<Position>
    {
        public PositionBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override Position DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var id = msg.GetValue<string>("identifier");
            var secKey = deserializer.FromField<IdentifierBundle>(msg.GetByName("securityKey"));
            var quant = msg.GetValue<string>("quantity");

            return new Position( UniqueIdentifier.Parse(id), long.Parse(quant), secKey);
        }
    }

    [FudgeSurrogate(typeof(PositionBuilder))]
    public class Position
    {
        private readonly IdentifierBundle _securityKey;
        private readonly UniqueIdentifier _identifier;
        private readonly long _quantity;

        public Position(UniqueIdentifier identifier, long quantity, IdentifierBundle securityKey)
        {
            _securityKey = securityKey;
            _identifier = identifier;
            _quantity = quantity;
        }

        public IdentifierBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public UniqueIdentifier Identifier
        {
            get { return _identifier; }
        }

        public long Quantity
        {
            get { return _quantity; }
        }
    }

    public class RemoteConfig
    {
        private readonly string _configId;
        private readonly RESTMagic _rootRest;
        private readonly FudgeMsg _configsMsg;
        private readonly string _userDataUri;
        private readonly string _viewProcessorUri;
        private string _securitySourceUri;
        private string _activeMQSpec;

        public RemoteConfig(string configId, string rootUri)
        {
            _rootRest = new RESTMagic(rootUri);
            _configId = configId;

            _configsMsg = _rootRest.GetSubMagic("configuration").GetReponse();

            var configMsg = ((IFudgeFieldContainer)_configsMsg.GetByName(_configId).Value);

            _activeMQSpec = configMsg.GetValue<string>("activeMQ");

            var serviceUris = GetServiceUris(configMsg, "userData", "viewProcessor", "securitySource");
            _userDataUri = serviceUris["userData"];
            _viewProcessorUri = serviceUris["viewProcessor"];
            _securitySourceUri = serviceUris["securitySource"];
            
            
        }



        public RemoteClient UserClient
        {
            get
            {
                return new RemoteClient(new RESTMagic(_userDataUri));
            }
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(new RESTMagic(_viewProcessorUri), _activeMQSpec);
            }
        }

        public RemoteSecuritySource SecuritySource
        {
            get {
                return new RemoteSecuritySource(new RESTMagic(_securitySourceUri));
            }
        }

        private Dictionary<string, string> GetServiceUris(IFudgeFieldContainer configMsg, params string[] serviceId)
        {
            return serviceId.AsParallel().ToDictionary(s=> s, s => GetServiceUri(configMsg, s));
        }



        private string GetServiceUri(IFudgeFieldContainer configMsg, string serviceId)
        {
            

            var userDataField = (FudgeMsg)configMsg.GetByName(serviceId).Value;

            var uris = new List<string>();
            foreach (var field in userDataField.GetAllFields())
            {
                switch (field.Name)
                {
                    case "type":
                        if (!"Uri".Equals((string)field.Value))
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case "uri":
                        uris.Add((string)field.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return GetWorkingUri(uris);
        }

        private static string GetWorkingUri(IEnumerable<string> uris)
        {
            return uris.AsParallel().Select(
                uri =>
                {
                    try
                    {
                        var webRequest = WebRequest.Create(uri);
                        //webRequest.Timeout = 5000;
                        using (webRequest.GetResponse())
                        { }
                    }
                    catch (WebException e)
                    {
                        if (e.Response is HttpWebResponse && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.MethodNotAllowed)
                        {
                            return uri;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                    return uri;

                }).Where(u => u != null).First();
        }

    }

    public class RemoteSecuritySource
    {
        private readonly RESTMagic _restMagic;

        public RemoteSecuritySource(RESTMagic restMagic)
        {
            _restMagic = restMagic;
        }

        public Security GetSecurity(UniqueIdentifier uid)
        {//TODO use this
            var fudgeMsg = _restMagic.GetSubMagic("securities").GetSubMagic("security").GetSubMagic(uid.ToString()).GetReponse();
            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            return fudgeSerializer.Deserialize<Security> (fudgeMsg); 
        }

       private class OrderedComparison<T> : IEqualityComparer<List<T>>
       {
           public static readonly OrderedComparison<T> Instance = new OrderedComparison<T>();

           public bool Equals(List<T> x, List<T> y)
           {
               return x.SequenceEqual(y);
           }

           public int GetHashCode(List<T> obj)
           {
               return obj[0].GetHashCode();
           }
       }
       readonly Dictionary<List<Identifier>, List<Security>> _securitiesCache = new Dictionary<List<Identifier>, List<Security>>(OrderedComparison<Identifier>.Instance);

        public List<Security> GetSecurities(IEnumerable<Identifier> idEnum)
        {
            var ids= idEnum.ToList();

            List<Security> ret;
            if (_securitiesCache.TryGetValue(ids, out ret))
            {
                return ret;
            }


            var parameters = ids.Select(s => new Tuple<string,string>("id", s.ToString())).ToArray();
            var fudgeMsg = _restMagic.GetSubMagic("securities", parameters).GetReponse();

            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            ret = fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<Security>).ToList();


            _securitiesCache[ids] = ret;

            return ret.ToList();
        }
    }

    public class RemoteViewProcessor
    {
        private readonly RESTMagic _rest;
        private readonly string _activeMqSpec;

        public RemoteViewProcessor(RESTMagic rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public IEnumerable<string> ViewNames
        {
            get
            {
                var fudgeMsg = _rest.GetSubMagic("viewNames").GetReponse();

                return fudgeMsg.GetAllByOrdinal(1).Select(fudgeField => (string) fudgeField.Value);
            }
        }

        public RemoteViewResource GetView(string viewName)
        {
            return new RemoteViewResource(_rest.GetSubMagic("views").GetSubMagic(viewName), _activeMqSpec);
        }
    }

    public class RemoteViewResource
    {
        private readonly RESTMagic _rest;
        private readonly string _activeMqSpec;

        public RemoteViewResource(RESTMagic rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public void Init()
        {
            var fudgeMsg = _rest.GetSubMagic("init").GetReponse("POST");
        }

        public Portfolio Portfolio
        {
            get
            {
                var fudgeMsg = _rest.GetSubMagic("portfolio").GetReponse();
                if (fudgeMsg == null)
                {
                    return null;
                }

                FudgeSerializer fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                return fudgeSerializer.Deserialize<Portfolio>(fudgeMsg);
            }
        }

        public ViewDefinition Definition
        {
            get
            {
                var fudgeMsg = _rest.GetSubMagic("definition").GetReponse();
              return FudgeConfig.GetFudgeSerializer().Deserialize<ViewDefinition>(fudgeMsg);
            }
        }

        public ViewClient CreateClient()
        {
            
            var clientUri = _rest.GetSubMagic("clients").Create(FudgeConfig.GetFudgeContext(), FudgeConfig.GetFudgeSerializer().SerializeToMsg(new UserPrincipal("bbgintegrationtestuser", GetIP())));

            return new ViewClient(clientUri, _activeMqSpec);
        }

        private string GetIP()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);
            int nIP = 0;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                return ipaddress.ToString();
            }
            throw new ArgumentException();
        }
    }

    namespace Mappedtypes
    {
        namespace financial.analytics
        {
            public class LabelledMatrixEntry
            {
                private readonly object _label;
                private readonly double _value;

                public LabelledMatrixEntry(object label, double value)
                {
                    _label = label;
                    _value = value;
                }

                public object Label
                {
                    get { return _label; }
                }

                public double Value
                {
                    get { return _value; }
                }
            }
            public class DoubleLabelledMatrix1D : IEnumerable<LabelledMatrixEntry>
            {
                private readonly IList<double> _keys;
                private readonly IList<object> _labels;
                private readonly IList<double> _values;

                private DoubleLabelledMatrix1D(IList<double> keys, IList<object> labels, IList<double> values)
                {
                    _keys = keys;
                    _labels = labels;
                    _values = values;
                }

                public IList<double> Keys
                {
                    get { return _keys; }
                }

                public IList<object> Labels
                {
                    get { return _labels; }
                }

                public IList<double> Values
                {
                    get { return _values; }
                }

                public static DoubleLabelledMatrix1D FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
                {
                    var keys = GetArray<double>(ffc, "keys");//This is a java Double[] go go go poor generics
                    var values = ffc.GetValue<double[]>("values");//This is a java (and hence a fudge) double[]
                    var labels = GetArray<object>(ffc, "labels");


                    return new DoubleLabelledMatrix1D(keys, labels, values);
                }

                /// <summary>
                /// Array here are packed YAN way
                /// </summary>
                private static List<T> GetArray<T>(IFudgeFieldContainer ffc, string fieldName)
                {
                    var fudgeFields = ffc.GetMessage(fieldName).GetAllFields();

                    return fudgeFields.Select(fudgeField => (T) fudgeField.Value).ToList();
                }

                public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
                {
                    throw new NotImplementedException();
                }

                public IEnumerator<LabelledMatrixEntry> GetEnumerator()
                {
                    return GetEntries().GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                private IEnumerable<LabelledMatrixEntry> GetEntries()
                {
                    return _labels.Zip(_keys, (l, k) => new LabelledMatrixEntry(l, k));
                }
            }

        }
        namespace financial.model.interestrate.curve
        {
            public class YieldCurve
            {
                public InterpolatedDoublesCurve Curve { get; set; }
            }
        }

        namespace LiveData
        {
            [Serializable]
            public class UserPrincipal
            {
                private string userName;
                private string ipAddress;

                public string UserName
                {
                    get { return userName; }
                    set { userName = value; }
                }

                public string IpAddress
                {
                    get { return ipAddress; }
                    set { ipAddress = value; }
                }

                public UserPrincipal(string userName, string ipAddress)
                {
                    UserName = userName;
                    IpAddress = ipAddress;
                }
            }
        }
    }

    /// <summary>
    /// DataViewClientResource
    /// </summary>
    public class ViewClient : DisposableBase
    {
        private readonly string _activeMqSpec;
        private readonly RESTMagic _rest;

        public ViewClient(Uri clientUri, string activeMqSpec)
        {
            _activeMqSpec = activeMqSpec;
            _rest = new RESTMagic(clientUri);
        }

        public void Start()
        {
            _rest.GetSubMagic("start").GetReponse("POST");
        }
        public void Stop()
        {
            _rest.GetSubMagic("stop").GetReponse("POST");
        }

        public void Pause()//TODO this
        {
            _rest.GetSubMagic("pause").GetReponse("POST");
        }

        public ClientResultStream<ViewComputationResultModel> StartResultStream()
        {
            var reponse = _rest.GetSubMagic("startJmsResultStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        public void StopResultStream()
        {
            _rest.GetSubMagic("startJmsResultStream").GetReponse("POST");
        }

        public ClientResultStream<ViewComputationResultModel> StartDeltaStream()
        {
            var reponse = _rest.GetSubMagic("startJmsDeltaStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        public void StopDeltaStream()
        {
            _rest.GetSubMagic("startJmsDeltaStream").GetReponse("POST");
        }

        public bool ResultAvailable
        {
            get {

                var reponse = _rest.GetSubMagic("resultAvailable").GetReponse();
                return 1 == (sbyte) (reponse.GetByName("value").Value);
            }
        }
        public ViewComputationResultModel LatestResult
        {
            get
            {
                var restMagic = _rest.GetSubMagic("latestResult").GetReponse();
                //ViewComputationResultModel
                var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                var wrapper = fudgeSerializer.Deserialize<Wrapper>(restMagic);
                return wrapper.LatestResult;
            }
        }
        public class Wrapper
        {
            public ViewComputationResultModel LatestResult { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
        }
    }

    public abstract class DisposableBase : IDisposable
    {
        ~DisposableBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }


    public class ViewComputationResultModelBuilder : BuilderBase<ViewComputationResultModel>
    {
        public ViewComputationResultModelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ViewComputationResultModel DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var viewName = msg.GetValue<String>("viewName");
            var inputDataTimestamp = msg.GetValue<FudgeDateTime>("valuationTS");
            var resultTimestamp = msg.GetValue<FudgeDateTime>("resultTS");
            var configurationMap = new Dictionary<String, ViewCalculationResultModel>();
            var keys = new Queue<String>();
            var values = new Queue<ViewCalculationResultModel>();

            foreach (var field in (IFudgeFieldContainer) msg.GetByName("results").Value)
            {
                switch (field.Ordinal)
                {
                    case 1:
                        String key = field.GetString();
                        if (!values.Any())
                        {
                            keys.Enqueue(key);
                        }
                        else
                        {
                            configurationMap.Add(key, values.Dequeue());
                        }
                        break;
                    case 2:
                        var value = FudgeConfig.GetFudgeSerializer().Deserialize<ViewCalculationResultModel>((FudgeMsg) field.Value);
                        if (!keys.Any())
                        {
                            values.Enqueue(value);
                        }
                        else
                        {
                            configurationMap.Add(keys.Dequeue(), value);
                        }
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            var targetMap = new Dictionary<ComputationTargetSpecification, ViewTargetResultModelImpl>();
            foreach (var configurationEntry in configurationMap)
            {
                foreach (ComputationTargetSpecification targetSpec in configurationEntry.Value.getAllTargets())
                {

                    ViewTargetResultModelImpl targetResult;

                    if (! targetMap.TryGetValue(targetSpec, out targetResult))
                    {
                        targetResult = new ViewTargetResultModelImpl();
                        targetMap.Add(targetSpec, targetResult);
                    }
                    targetResult.AddAll(configurationEntry.Key, configurationEntry.Value[targetSpec]);
                }
            }
    
            var allResults = new List<ViewResultEntry>();
            foreach (var configurationEntry in configurationMap)
            {
                foreach (var targetSpec in configurationEntry.Value.getAllTargets())
                {
                    var results = configurationEntry.Value[targetSpec];
                    foreach (var value in results)
                    {
                        allResults.Add(new ViewResultEntry(configurationEntry.Key, value.Value));
                    }
                }
            }
            
            return new ViewComputationResultModel(viewName, inputDataTimestamp, resultTimestamp, configurationMap, targetMap, allResults);
        }
    }

    public class ViewResultEntry
    {

        private readonly string _calculationConfiguration;
        private readonly ComputedValue _computedValue;

        public ViewResultEntry(string calculationConfiguration, ComputedValue computedValue)
        {
            _calculationConfiguration = calculationConfiguration;
            _computedValue = computedValue;
        }

        public string CalculationConfiguration
        {
            get { return _calculationConfiguration; }
        }

        public ComputedValue ComputedValue
        {
            get { return _computedValue; }
        }
    }

    public class ViewTargetResultModelImpl : ViewTargetResultModel
    {
        private readonly Dictionary<string, Dictionary<string, ComputedValue>> _inner = new Dictionary<string, Dictionary<string, ComputedValue>>();
        public void AddAll(string key, Dictionary<string, ComputedValue> values)
        {
            _inner.Add(key, values);
        }
    }

    [FudgeSurrogate(typeof(ViewCalculationResultModelBuilder))]
    public class ViewCalculationResultModel
    {
        private readonly Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> _map;

        public ViewCalculationResultModel(Dictionary<ComputationTargetSpecification, Dictionary<string, ComputedValue>> map)
        {
            _map = map;
        }

        public ICollection<ComputationTargetSpecification> getAllTargets()
        {
            return _map.Keys;
        }

        public Dictionary<String, ComputedValue> this[ComputationTargetSpecification target]
        {
            get { return _map[target]; }
        }
    }


    public class ViewCalculationResultModelBuilder: BuilderBase<ViewCalculationResultModel>
    {
        public ViewCalculationResultModelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }
        public override ViewCalculationResultModel DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var map = new Dictionary<ComputationTargetSpecification, Dictionary<String, ComputedValue>>();
            foreach (var field in msg)
            {
                var subMsg = (IFudgeFieldContainer) field.Value;
                
                var valueSpecification = deserializer.FromField<ValueSpecification>(subMsg.GetByName("specification"));
                object innerValue = GetValue(deserializer, subMsg, valueSpecification);

                var value = new ComputedValue(valueSpecification, innerValue);
                
                ComputationTargetSpecification target = value.Specification.TargetSpecification;
                if (!map.ContainsKey(target)) {
                    map.Add(target, new Dictionary<String, ComputedValue>());
                }
                map[target].Add(value.Specification.ValueName, value);
            }
            return new ViewCalculationResultModel(map);
        }

        private static object GetValue(IFudgeDeserializer deserializer, IFudgeFieldContainer subMsg, ValueSpecification valueSpecification)
        {
            var o = subMsg.GetByName("value");
            object innerValue;

            if (valueSpecification.ValueName == "YieldCurveJacobian")
            {
                var fromField = deserializer.FromField<List<double[]>>(o);
                return fromField;//TODO I hope this gets a better type one day?
            }

            
            var t = o.Type.CSharpType;
            if (o.Type == FudgeMsgFieldType.Instance || o.Type == IndicatorFieldType.Instance)
            {
                innerValue = deserializer.FromField(o, t);
            }
            else
            {
                innerValue = subMsg.GetValue("value");
            }
            
            return innerValue;
        }
    }


    public class ComputedValue
    {
        private readonly ValueSpecification _specification;
        private readonly object _value;

        public ComputedValue(ValueSpecification specification, object value)
        {
            _specification = specification;
            _value = value;
        }

        public ValueSpecification Specification
        {
            get { return _specification; }
        }

        public object Value
        {
            get { return _value; }
        }
    }

    
    [FudgeSurrogate(typeof(ValueSpecificationBuilder))]
    public class ValueSpecification : IEquatable<ValueSpecification>
    {
        private readonly string _valueName;
        private readonly ComputationTargetSpecification _targetSpecification;

        public ValueSpecification(string valueName, ComputationTargetSpecification targetSpecification)
        {
            _valueName = valueName;
            _targetSpecification = targetSpecification;
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public ComputationTargetSpecification TargetSpecification
        {
            get { return _targetSpecification; }
        }


        public bool Equals(ValueSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._valueName, _valueName) && Equals(other._targetSpecification, _targetSpecification);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ValueSpecification)) return false;
            return Equals((ValueSpecification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_valueName != null ? _valueName.GetHashCode() : 0)*397) ^ (_targetSpecification != null ? _targetSpecification.GetHashCode() : 0);
            }
        }
    }

    public abstract class BuilderBase<T>: IFudgeSerializationSurrogate
    {
        protected readonly FudgeContext _context;
        protected readonly Type _type;

        protected BuilderBase(FudgeContext context, Type type)
        {
            _context = context;
            _type = type;
        }

        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var ret = DeserializeImpl(msg, deserializer);
            deserializer.Register(msg, ret);
            return ret;
        }

        public abstract T DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
    }
    public class ValueSpecificationBuilder : BuilderBase<ValueSpecification>
    {
        public ValueSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ValueSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var valueName = msg.GetValue<String>("valueName");
            var targetSpecification = new ComputationTargetSpecificationBuilder(_context, typeof(ComputationTargetSpecification)).DeserializeImpl(msg, deserializer); //Can't register twice
            //TODO properties
            return new ValueSpecification(valueName, targetSpecification);
        }
    }

    public class ComputationTargetSpecificationBuilder : BuilderBase<ComputationTargetSpecification>
    {
        public ComputationTargetSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ComputationTargetSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            ComputationTargetType type;
            if (! Enum.TryParse(msg.GetValue<String>("computationTargetType"), out type))
            {
                throw new Exception("Unhandled computation target type");
            }
            UniqueIdentifier uid = null;
            var ctiField = msg.GetByName("computationTargetIdentifier");
            if (ctiField !=null) {
                uid = UniqueIdentifier.Parse(msg.GetValue<String>("computationTargetIdentifier"));
            }
            return new ComputationTargetSpecification(type, uid);
        }
    }

public enum ComputationTargetType {

  /**
   * A set of positions (a portfolio node, or whole portfolio).
   */
  PORTFOLIO_NODE,
  /**
   * A position.
   */
  POSITION,
  /**
   * A security.
   */
  SECURITY,
  /**
   * A simple type, effectively "anything else".
   */
  PRIMITIVE,
  /**
   * A trade.
   */
  TRADE

}
    [FudgeSurrogate(typeof(ComputationTargetSpecificationBuilder))]
    public  class ComputationTargetSpecification
    {
        private readonly ComputationTargetType _type;
        private readonly UniqueIdentifier _uid;

        public ComputationTargetSpecification(ComputationTargetType type, UniqueIdentifier uid)
        {
            _type = type;
            _uid = uid;
        }

        public ComputationTargetType Type
        {
            get { return _type; }
        }

        public UniqueIdentifier Uid
        {
            get { return _uid; }
        }

        public bool Equals(ComputationTargetSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._type, _type) && Equals(other._uid, _uid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ComputationTargetSpecification)) return false;
            return Equals((ComputationTargetSpecification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode()*397) ^ (_uid != null ? _uid.GetHashCode() : 0);
            }
        }
    }

    public class Instant
    {
        private readonly long _seconds;
        private readonly int _nanos;

        public Instant(long seconds, int nanos)
        {
            _seconds = seconds;
            _nanos = nanos;
        }

        public long Seconds
        {
            get { return _seconds; }
        }

        public int Nanos
        {
            get { return _nanos; }
        }
    }

    [FudgeSurrogate(typeof(ViewComputationResultModelBuilder))]
    public class ViewComputationResultModel
    {
        private readonly string _viewName;
        private readonly FudgeDateTime _inputDataTimestamp;
        private readonly FudgeDateTime _resultTimestamp;
        private readonly Dictionary<string, ViewCalculationResultModel> _configurationMap;
        private readonly Dictionary<ComputationTargetSpecification, ViewTargetResultModelImpl> _targetMap;
        private readonly List<ViewResultEntry> _allResults;

        public ViewComputationResultModel(string viewName, FudgeDateTime inputDataTimestamp, FudgeDateTime resultTimestamp, Dictionary<string, ViewCalculationResultModel> configurationMap, Dictionary<ComputationTargetSpecification, ViewTargetResultModelImpl> targetMap, List<ViewResultEntry> allResults)
        {
            _viewName = viewName;
            _inputDataTimestamp = inputDataTimestamp;
            _resultTimestamp = resultTimestamp;
            _configurationMap = configurationMap;
            _targetMap = targetMap;
            _allResults = allResults;
        }

        public ICollection<ComputationTargetSpecification> AllTargets
        {
            get { return _targetMap.Keys; }
        }

        public ICollection<string> CalculationConfigurationNames { get { return _configurationMap.Keys; } }
        public ViewCalculationResultModel GetCalculationResult(string calcConfigurationName)
        {
            return _configurationMap[calcConfigurationName];
        }
        public ViewTargetResultModel getTargetResult(ComputationTargetSpecification target)
        {
            return _targetMap[target];
        }
        public FudgeDateTime ValuationTime { get { return _inputDataTimestamp; } }
        public FudgeDateTime ResultTimestamp { get { return _resultTimestamp; } }
        public String ViewName { get { return _viewName; } }
        public IList<ViewResultEntry> AllResults{ get { return _allResults; } }

        public ViewComputationResultModel ApplyDelta(ViewComputationResultModel delta)
        {
            //TODO if (delta._inputDataTimestamp. < _inputDataTimestamp) throw
            //TODO assert config map same and targetmap subset

            var deltaResults = delta._allResults.ToDictionary(r => new Tuple<string, ValueSpecification>(r.CalculationConfiguration, r.ComputedValue.Specification), r => r);


            var results = new List<ViewResultEntry>(_allResults.Count);

            foreach (var pair in _allResults.Select(r => new Tuple<Tuple<string, ValueSpecification>, ViewResultEntry>(
                    new Tuple<string, ValueSpecification>(r.CalculationConfiguration, r.ComputedValue.Specification), r)))
            {
                var key = pair.Item1;
                var vre = pair.Item2;
                ViewResultEntry newValue;
                if (deltaResults.TryGetValue(key, out newValue))
                {
                    deltaResults.Remove(key);
                }
                else
                {
                    newValue = vre;
                }
                results.Add(newValue);
            }
            results.AddRange(deltaResults.Select(kvp => kvp.Value));
            
            return new ViewComputationResultModel(_viewName, delta._inputDataTimestamp, delta._resultTimestamp, _configurationMap, _targetMap, results);
        }
    }


    public class LaxTypeMappingStrategy : IFudgeTypeMappingStrategy
    {
        private readonly Assembly _assembly;

        public LaxTypeMappingStrategy(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string GetName(Type type)
        {
            //throw new NotImplementedException();
            return null;
        }

        public Type GetType(string fullName)
        {
            var name = fullName.Substring(fullName.LastIndexOf(".") + 1);
            foreach (var type in _assembly.GetTypes())
            {
                if (type.Name == name)
                {
                    return type;
                }
                else if ((type.Name + "Impl") == name)
                {
                    return type;
                }
            }
            return null;
        }
    }

    public class RemoteClient
    {
        

        private readonly string _clientId;
        private readonly RESTMagic _rest;

        public RemoteClient(RESTMagic userDataRest)
            : this(userDataRest, Environment.UserName, Guid.NewGuid().ToString())
        {
        }

        public RemoteClient(RESTMagic userDataRest, string username, string clientId)
        {
            _clientId = clientId;
            _rest = userDataRest.GetSubMagic(username).GetSubMagic("clients").GetSubMagic(_clientId);
        }


        public Action HeartbeatSender {
            get { return () => _rest.GetSubMagic("heartbeat").GetReponse("POST"); }
        }
    }
}

