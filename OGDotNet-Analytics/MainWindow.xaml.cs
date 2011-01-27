using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var remoteConfig = new RemoteConfig("0", "http://localhost:8080/jax/");
            var remoteClient = remoteConfig.UserClient;
            remoteClient.HeartbeatSender();
            var remoteViewProcessor = remoteConfig.ViewProcessor;
            var viewNames = remoteViewProcessor.ViewNames;
            foreach (var viewName in viewNames.Where(v => v == "Swap Test View"))
            {
                var remoteViewResource = remoteViewProcessor.GetView(viewName);
                remoteViewResource.Init();
                var portfolio = remoteViewResource.Portfolio;
                var viewDefinition = remoteViewResource.Definition;
                var client= remoteViewResource.CreateClient();
                client.Start();
                while (! client.ResultAvailable)
                {
                    
                }
                var latestResult = client.LatestResult;
            }
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

    public class Position
    {
        public string Identity { get; set; }
        public string Quantity { get; set; }
        public UniqueIdentifier SecurityKey { get; set; }
    }

    public class RemoteConfig
    {
        private readonly string _configId;
        private readonly RESTMagic _rootRest;
        private readonly FudgeMsg _configMsg;
        private string _userDataUri;
        private string _viewProcessorUri;

        public RemoteConfig(string configId, string rootUri)
        {
            _rootRest = new RESTMagic(rootUri);
            _configId = configId;

            _configMsg = _rootRest.GetSubMagic("configuration").GetReponse();

            _userDataUri = GetServiceUri(_configMsg, "userData");
            _viewProcessorUri = GetServiceUri(_configMsg, "viewProcessor");
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
                return new RemoteViewProcessor(new RESTMagic(_viewProcessorUri));
            }
        }

        private string GetServiceUri(FudgeMsg config, string serviceId)
        {
            FudgeMsg userDataField = (FudgeMsg)((IFudgeFieldContainer)config.GetByName(_configId).Value).GetByName(serviceId).Value;

            List<string> uris = new List<string>();
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

            //TODO string serviceUri = GetWorkingUri(uris);
            return GetWorkingUri(uris.Where(u => u.Contains("2.")));
        }

        private static string GetWorkingUri(IEnumerable<string> uris)
        {
            return uris.AsParallel().Select(
                uri =>
                {
                    try
                    {
                        using (WebRequest.Create(uri).GetResponse())
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

    public class RemoteViewProcessor
    {
        private readonly RESTMagic _rest;

        public RemoteViewProcessor(RESTMagic rest)
        {
            _rest = rest;
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
            return new RemoteViewResource(_rest.GetSubMagic("views").GetSubMagic(viewName));
        }
    }

    public class RemoteViewResource
    {
        private readonly RESTMagic _rest;

        public RemoteViewResource(RESTMagic rest)
        {
            _rest = rest;
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

            return new ViewClient(clientUri);
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
    public static class FudgeConfig
    {
        public static FudgeSerializer GetFudgeSerializer()
        {
            FudgeContext fudgeContext = GetFudgeContext();

            return new FudgeSerializer(fudgeContext);
        }

        public static FudgeContext GetFudgeContext()
        {
            var fudgeContext = new FudgeContext();
            IFudgeTypeMappingStrategy old = (IFudgeTypeMappingStrategy)fudgeContext.GetProperty(ContextProperties.TypeMappingStrategyProperty, new JoiningMappingStrategty());
            fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty, new JoiningMappingStrategty(old, new JavaTypeMappingStrategy("OGDotNet_Analytics.Mappedtypes", "com.opengamma"), new LaxTypeMappingStrategy()));
            fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
            return fudgeContext;
        }
    }

    namespace Mappedtypes.LiveData
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

    /// <summary>
    /// DataViewClientResource
    /// </summary>
    public class ViewClient
    {
        private readonly RESTMagic _rest;

        public ViewClient(Uri clientUri)
        {
            _rest = new RESTMagic(clientUri);
        }
        public void Start()
        {
            var reponse = _rest.GetSubMagic("start").GetReponse("POST");
        }

        public bool ResultAvailable
        {
            get {

                var reponse = _rest.GetSubMagic("resultAvailable").GetReponse();
                return 1 == (sbyte) (reponse.GetByName("value").Value);
            }
        }
        public object LatestResult
        {
            get
            {
                var restMagic = _rest.GetSubMagic("latestResult").GetReponse();
                //ViewComputationResultModel
                var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                var viewComputationResultModel = fudgeSerializer.Deserialize<ViewComputationResultModel>(restMagic.GetValue<FudgeMsg>("latestResult"));
                return viewComputationResultModel;
            }
        }
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
                var value = deserializer.FromField<ComputedValue>(field);
                ComputationTargetSpecification target = value.Specification.TargetSpecification;
                if (!map.ContainsKey(target)) {//TODO try get
                    map.Add(target, new Dictionary<String, ComputedValue>());
                }
                map[target].Add(value.Specification.ValueName, value);
            }
            return new ViewCalculationResultModel(map);
        }
    }

    public class ComputedValue
    {//TODO
        public ValueSpecification Specification { get; set; }
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

        protected BuilderBase(FudgeContext context, Type type)
        {
            _context = context;
            if (type != typeof(T))
            {
                throw new ArgumentException();
            }
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

    
    public class ViewTargetResultModel
    {
        //TODO
    }

    public class ViewDefinition
    {
        public ResultModelDefinition ResultModelDefinition { get; set; }
    }
    public class ResultModelDefinition
    {
        
    }

    public class LaxTypeMappingStrategy : IFudgeTypeMappingStrategy
    {
        public string GetName(Type type)
        {
            //throw new NotImplementedException();
            return null;
        }

        public Type GetType(string fullName)
        {
            var name = fullName.Substring(fullName.LastIndexOf(".") + 1);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
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
