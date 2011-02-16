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


        private static IDictionary<string, Uri> GetServiceUris(IFudgeFieldContainer configMsg, params string[] serviceIds)
        {
            var validServiceUris = new Dictionary<string, Uri>();

            var asyncRequests = new List<Tuple<String,HttpWebRequest, IAsyncResult>>();
            
            foreach (var serviceId in serviceIds)
            {
                foreach (var uri in GetPotentialUris(configMsg, serviceId))
                {
                    var webRequest = (HttpWebRequest) WebRequest.Create(uri);
                    webRequest.Timeout = 5000;

                    var result = webRequest.BeginGetResponse(null,serviceId);
                    asyncRequests.Add(new Tuple<string, HttpWebRequest,IAsyncResult>(serviceId,webRequest,result));
                }
            }

            
            while (validServiceUris.Count < serviceIds.Length)
            {
                if (! asyncRequests.Any())
                {
                    var missingKeys = string.Join(",", serviceIds.Except(validServiceUris.Keys));
                    throw new ArgumentException(string.Format("Couldn't get service Uri for {0}", missingKeys));
                }

                var waitHandles = asyncRequests.Select(kvp => kvp.Item3.AsyncWaitHandle).ToArray();
                var index = WaitHandle.WaitAny(waitHandles);
                var completedRequest = asyncRequests[index];
                asyncRequests.RemoveAt(index);

                if (IsValidResponse(completedRequest))
                {
                    //TODO: does it matter if another uri was faster (from this one sample)?
                    validServiceUris[completedRequest.Item1] = completedRequest.Item2.RequestUri;
                }
            }


            //We need to clear up, but we don't care about the results
            foreach (var req in asyncRequests)
            {
                req.Item2.Abort();
            }
            foreach (var req in asyncRequests)
            {
                IsValidResponse(req);
            }

            return validServiceUris;
        }

        private static bool IsValidResponse(Tuple<string, HttpWebRequest, IAsyncResult> tuple)
        {
            try
            {
                tuple.Item2.EndGetResponse(tuple.Item3).Close();
                return true;
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static IEnumerable<string> GetPotentialUris(IFudgeFieldContainer configMsg, string serviceId)
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
            return uris;
        }

        #endregion

    }
}