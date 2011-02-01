using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Fudge;


namespace OGDotNet_Analytics.Model.Resources
{
    public class RemoteConfig
    {
        private readonly string _configId;
        private readonly RestTarget _rootRest;
        private readonly FudgeMsg _configsMsg;
        private readonly string _userDataUri;
        private readonly string _viewProcessorUri;
        private string _securitySourceUri;
        private string _activeMQSpec;

        public RemoteConfig(string configId, string rootUri)
        {
            _rootRest = new RestTarget(rootUri);
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
                return new RemoteClient(new RestTarget(_userDataUri));
            }
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(new RestTarget(_viewProcessorUri), _activeMQSpec);
            }
        }

        public RemoteSecuritySource SecuritySource
        {
            get {
                return new RemoteSecuritySource(new RestTarget(_securitySourceUri));
            }
        }

        private static Dictionary<string, string> GetServiceUris(IFudgeFieldContainer configMsg, params string[] serviceId)
        {
            return serviceId.AsParallel().ToDictionary(s => s, s => GetServiceUri(configMsg, s));
        }



        private static string GetServiceUri(IFudgeFieldContainer configMsg, string serviceId)
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
            return uris.OrderBy(PrefferenceOrder).Select(
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
                        catch (Exception)
                        {
                            return null;
                        }
                        return uri;

                    }).Where(u => u != null).First();
        }
       private static int PrefferenceOrder(string uri)
       {
           switch (new Uri(uri).HostNameType)
           {
               case UriHostNameType.IPv4:
                   return 0;
               case UriHostNameType.Basic:
               case UriHostNameType.Dns:
                   return 5;
               case UriHostNameType.Unknown:
               case UriHostNameType.IPv6:
                   return 10;
               default:
                   throw new ArgumentOutOfRangeException();
           }
       }

    }
}