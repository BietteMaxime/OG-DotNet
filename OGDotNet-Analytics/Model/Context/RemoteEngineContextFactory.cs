using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Fudge;
using OGDotNet.Properties;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContextFactory
    {
        /// <summary>
        /// TODO: this is a hack, kill it
        /// </summary>
        public static RemoteEngineContextFactory DefaultRemoteEngineContextFactory
        {
              get
              {
                  return new RemoteEngineContextFactory(Settings.Default.ServiceUri, Settings.Default.ConfigId);
              }  
        } 

        private readonly Uri _rootUri;
        private readonly string _configId;
        private readonly RestTarget _rootRest;
        private readonly Config _config;
        
        public RemoteEngineContextFactory(string rootUri, string configId)
        {
            _rootUri = new Uri(rootUri);
            _configId = configId;
            _rootRest = new RestTarget(rootUri);
            _config = InitConfig();
        }

        public RemoteEngineContext CreateRemoteEngineContext()
        {
            return new RemoteEngineContext(_config);
        }

        #region ConfigReading
        private Config InitConfig()
        {
            var configsMsg = _rootRest.Resolve("configuration").GetReponse();

            var configMsg = ((IFudgeFieldContainer)configsMsg.GetByName(_configId).Value);

            var activeMQSpec = configMsg.GetValue<string>("activeMQ");

            var serviceUris = GetServiceUris(configMsg, "userData", "viewProcessor", "securitySource");
            var userDataUri = serviceUris["userData"];
            var viewProcessorUri = serviceUris["viewProcessor"];
            var securitySourceUri = serviceUris["securitySource"];

            return new Config(_rootUri, activeMQSpec, userDataUri, viewProcessorUri, securitySourceUri);
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
        #endregion

    }
}