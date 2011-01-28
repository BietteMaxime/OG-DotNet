using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet;
using OGDotNet_Analytics.Mappedtypes.LiveData;

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
            var remoteConfig = new RemoteConfig("0", "http://localhost:8080/jax"); //devsvr-lx-2 or localhost
            
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
                remoteViewResource.Init();

                cancellationToken.ThrowIfCancellationRequested();
                var viewDefinition = remoteViewResource.Definition;

                Dispatcher.Invoke((Action)(() =>
                   {
                       var viewBase = (GridView)table.View;

                       while (viewBase.Columns.Count >1)
                       {
                           viewBase.Columns.RemoveAt(1);                           
                       }

                       foreach (var column in GetColumns(viewDefinition))
                       {
                           viewBase.Columns.Add(new GridViewColumn
                                                    {
                                                        Width = Double.NaN,
                                                        Header = column,
                                                        DisplayMemberBinding = new Binding(string.Format(".[{0}]", column))//TODO bugs galore
                                                    });
                       }
                   }));


                cancellationToken.ThrowIfCancellationRequested();
                using (var client = remoteViewResource.CreateClient())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    client.Start();
                    using (var startResultStream = client.StartResultStream())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var portfolio = remoteViewResource.Portfolio;

                        while (! cancellationToken.IsCancellationRequested)
                        {
                            var results = startResultStream.GetNext(cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();
                            var rows = BuildRows(viewDefinition, results, portfolio).ToList();

                            cancellationToken.ThrowIfCancellationRequested();
                            Dispatcher.Invoke((Action) (() =>
                                                            {
                                                                cancellationToken.ThrowIfCancellationRequested();
                                                                table.DataContext = rows;

                                                                count.Text = _counter++.ToString();

                                                            }));
                        }

                    }
                }
            }
            catch (OperationCanceledException oce)
            {
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed to retrieve data");
            }
        }


        private static IEnumerable<string> GetColumns(ViewDefinition viewDefinition)
        {
            foreach (var configuration in viewDefinition.CalculationConfigurationsByName)
            {
                foreach (var valuePropertiese in configuration.Value.PortfolioRequirementsBySecurityType)
                {
                    foreach (var property in valuePropertiese.Value.Properties)
                    {
                        foreach (var p in property.Value)
                        {
                            yield return string.Format("{0}/{1}", configuration.Key, p);
                        }
                    }
                }
            }
        }

        private static IEnumerable<Position> GetPositions(Portfolio portfolio)
        {
            return GetPositions(portfolio.Root);
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

        private IEnumerable<Row> BuildRows(ViewDefinition viewDefinition, ViewComputationResultModel results, Portfolio portfolio)
        {
            Dictionary<Tuple<UniqueIdentifier, string, string>, object> valueIndex = Indexvalues(results);


            foreach (var position in GetPositions(portfolio))
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
                            object value;
                            if (
                                valueIndex.TryGetValue(
                                    new Tuple<UniqueIdentifier, string, string>(position.GetIdentifier(), portfolioReq,
                                                                                configuration.Key), out value))
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

                var securities =
                    _remoteSecuritySource.GetSecurities(
                        position.SecurityKey.Identifiers.Select(x => x.ToString()).ToList());
                if (securities.Count != 1)
                {
                    throw new ArgumentException();
                }

                string securityName = securities[0].Name;


                yield return new Row(position.GetIdentifier(), position.GetQuantity(), values, securityName);
            }
        }

        private static Dictionary<Tuple<UniqueIdentifier, string, string>, object> Indexvalues(ViewComputationResultModel results)
        {
            var valueIndex = new Dictionary<Tuple<UniqueIdentifier, string, string>, object>();
            foreach (var result in results.AllResults)
            {
                if (result.ComputedValue.Specification.TargetSpecification.Type == ComputationTargetType.POSITION)
                {
                    valueIndex.Add(
                        new Tuple<UniqueIdentifier, string, string>(
                            result.ComputedValue.Specification.TargetSpecification.Uid,
                            result.ComputedValue.Specification.ValueName, result.CalculationConfiguration),
                        result.ComputedValue.Value);
                }
            }
            return valueIndex;
        }

        public class Row
        {
            private readonly string _positionName;
            private readonly long _quantity;
            private readonly Dictionary<string, object> _columns;
            private readonly string _securityName;

            public Row(UniqueIdentifier positionId, long quantity, Dictionary<string, object> columns, string securityName)
            {
                _positionName = string.Format("{0} ({1})", securityName, quantity);
                _quantity = quantity;
                _columns = columns;
                _securityName = securityName;
            }


            public string SecurityName
            {
                get { return _securityName; }
            }

            public string PositionName
            {
                get
                {
                    return _positionName;
                }
            }

            public long Quantity
            {
                get { return _quantity; }
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
                get
                {
                    return _columns.ContainsKey(key) ? _columns[key] : "";//TODO
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //TODO this properly
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
        public UniqueIdentifier UniqueId { get; set; }
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

        public UniqueIdentifier GetIdentifier()//TODO make fudge happy with me with this as a property
        {
            return _identifier;
        }

        public long GetQuantity()//TODO make fudge happy with me with this as a property
        {
            return _quantity;
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

       private class OrderedComparison : IEqualityComparer<List<string>>
       {
           public static OrderedComparison Instance  =new OrderedComparison();

           public bool Equals(List<string> x, List<string> y)
           {
               return x.SequenceEqual(y);
           }

           public int GetHashCode(List<string> obj)
           {
               return obj[0].GetHashCode();
           }
       }
       readonly Dictionary<List<string>, List<Security>> _securitiesCache = new Dictionary<List<string>, List<Security>>(OrderedComparison.Instance);

        public List<Security> GetSecurities(List<String> idStrings)//TODO type
        {
            List<Security> ret;
            if (_securitiesCache.TryGetValue(idStrings, out ret))
            {
                return ret;
            }


            var parameters = idStrings.Select(s => new Tuple<string,string>("id", s)).ToArray();
            var fudgeMsg = _restMagic.GetSubMagic("securities", parameters).GetReponse();

            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            ret = fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<Security>).ToList();


            _securitiesCache[idStrings] = ret;

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
        namespace financial.model.interestrate.curve
        {
            public class YieldCurve
            {
                //TODO
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
            var reponse = _rest.GetSubMagic("start").GetReponse("POST");
        }
        public void Stop()//TODO dispose
        {
            var reponse = _rest.GetSubMagic("stop").GetReponse("POST");
        }
        public void Pause()
        {
            var reponse = _rest.GetSubMagic("pause").GetReponse("POST");
        }

        public ClientResultStream StartResultStream()
        {
            var reponse = _rest.GetSubMagic("startJmsResultStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream(queueUri, queueName, StopResultStream);
        }
        public void StopResultStream()
        {
            var reponse = _rest.GetSubMagic("startJmsResultStream").GetReponse("POST");
        }

        public void StartDeltaStream()//TODO use this
        {
            var reponse = _rest.GetSubMagic("startJmsDeltaStream").GetReponse("POST");
        }
        public void StopDeltaStream()
        {
            var reponse = _rest.GetSubMagic("startJmsDeltaStream").GetReponse("POST");
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
                    targetResult.AddAll(configurationEntry.Key, configurationEntry.Value.getValues(targetSpec));
                }
            }
    
            var allResults = new List<ViewResultEntry>();
            foreach (var configurationEntry in configurationMap)
            {
                foreach (var targetSpec in configurationEntry.Value.getAllTargets())
                {
                    var results = configurationEntry.Value.getValues(targetSpec);
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
        //TODO
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

        public Dictionary<String, ComputedValue> getValues(ComputationTargetSpecification target)
        { //TODO indexer?
            return _map[target];
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
                if (!map.ContainsKey(target)) {//TODO try get
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

            try
            {
                var t = o.Type.CSharpType;

                innerValue = deserializer.FromField(o, t);
            }
            catch (Exception e)
            {
                innerValue = new MisunderstoodValue(e);
            }
            return innerValue;
        }
    }

    public class VolatilitySurfaceData
    {//TODO
        
    }
    public class MisunderstoodValue
    {
        //TODO stop this
        private readonly Exception _exception;

        public MisunderstoodValue(Exception exception)
        {
            _exception = exception;
            Debug.Fail(exception.Message);
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
    public class ValueSpecification
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
