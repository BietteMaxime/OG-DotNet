//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactory.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Fudge;
using OGDotNet.Mappedtypes;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContextFactory
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _rootUri;
        private readonly string _configId;
        private readonly RestTarget _rootRest;

        //NOTE: don't do the blocking init in the constructor
        private readonly Lazy<IFudgeFieldContainer> _configMessage;
        private readonly Lazy<IDictionary<string, Uri>> _serviceUrisLazy;
        private readonly Lazy<string> _activeMQSpecLazy;

        public RemoteEngineContextFactory(OpenGammaFudgeContext fudgeContext, Uri rootUri, string configId)
        {
            _fudgeContext = fudgeContext;
            _rootUri = rootUri;
            _configId = configId;
            _rootRest = new RestTarget(_fudgeContext, rootUri);


            _configMessage = new Lazy<IFudgeFieldContainer>(GetConfigMessage);
            _activeMQSpecLazy = new Lazy<string>(() => _configMessage.Value.GetValue<string>("activeMQ"));
            _serviceUrisLazy = new Lazy<IDictionary<string, Uri>>(() => GetServiceUris(_configMessage.Value));
        }

        private IFudgeFieldContainer GetConfigMessage()
        {
            var msg = _rootRest.Resolve("configuration").GetFudge().GetMessage(_configId);
            if (msg==null)
            {
                throw new OpenGammaException("Missing config "+_configId);
            }
            return msg;
        }

        public RemoteEngineContext CreateRemoteEngineContext()
        {
            return new RemoteEngineContext(_fudgeContext, _rootUri, _activeMQSpecLazy.Value, _serviceUrisLazy.Value);
        }

        #region ConfigReading

        private static IDictionary<string, Uri> GetServiceUris(IFudgeFieldContainer configMsg)
        {
            Dictionary<string, List<string>> potentialServiceIds = GetPotentialServiceUris(configMsg);

            return GetValidServiceUris(potentialServiceIds);
        }

        private static Dictionary<string, List<string>> GetPotentialServiceUris(IFudgeFieldContainer configMsg)
        {
            var potentialServiceIds =new Dictionary<string, List<string>>();
            foreach (var userDataField in configMsg)
            {
                if (!(userDataField.Value is IFudgeFieldContainer))
                {
                    continue;
                }


                var uris = new List<string>();
                foreach (var field in ((IFudgeFieldContainer) userDataField.Value).GetAllFields())
                {
                    switch (field.Name)
                    {
                        case "type":
                            if (!"Uri".Equals((string)field.Value))
                            {
                                continue;
                            }
                            break;
                        case "uri":
                            uris.Add((string)field.Value);
                            break;
                        default:
                            continue;
                    }
                }
                
                potentialServiceIds.Add(userDataField.Name, uris);
            }
            return potentialServiceIds;
        }

        private static IDictionary<string, Uri> GetValidServiceUris(Dictionary<string, List<string>> potentialServiceIds)
        {
            var validServiceUris = new Dictionary<string, Uri>();

            var asyncRequests = new List<Tuple<string, HttpWebRequest, IAsyncResult>>();

            foreach (var potentialServiceId in potentialServiceIds)
            {
                var serviceId = potentialServiceId.Key;
                foreach (var uri in potentialServiceId.Value)
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);

                    var result = webRequest.BeginGetResponse(null, serviceId);
                    asyncRequests.Add(new Tuple<string, HttpWebRequest, IAsyncResult>(serviceId, webRequest, result));
                }
            }


            while (asyncRequests.Any())
            {
                var waitHandles = asyncRequests.Select(kvp => kvp.Item3.AsyncWaitHandle).ToArray();

                var index = WaitHandle.WaitAny(waitHandles, 5000); //Have to timeout by hand, see http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout.aspx
                if (index == WaitHandle.WaitTimeout)
                {
                    foreach (var req in asyncRequests)
                    {
                        req.Item2.Abort();
                    }
                    continue;
                }

                var completedRequest = asyncRequests[index];
                asyncRequests.RemoveAt(index);

                if (IsValidResponse(completedRequest))
                {
                    validServiceUris[completedRequest.Item1] = completedRequest.Item2.RequestUri;

                    foreach (var req in asyncRequests.Where(r => r.Item1 == completedRequest.Item1))
                    {
                        req.Item2.Abort();
                    }
                }
            }

            if (!validServiceUris.Any())
            {
                throw new WebException("Failed to get any service Uris");
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
                return e.Response is HttpWebResponse && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.MethodNotAllowed;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}