//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactory.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Fudge;
using OGDotNet.Mappedtypes;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContextFactory : LoggingClassBase
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
            Logger.Info("Getting configuration info for {0}", _rootUri);

            var msg = _rootRest.Resolve("configuration", _configId).GetFudge();
            if (msg == null)
            {
                throw new OpenGammaException("Missing config " + _configId);
            }
            return msg;
        }

        public RemoteEngineContext CreateRemoteEngineContext()
        {
            return new RemoteEngineContext(_fudgeContext, _rootUri, _activeMQSpecLazy.Value, _serviceUrisLazy.Value);
        }

        #region ConfigReading

        private IDictionary<string, Uri> GetServiceUris(IFudgeFieldContainer configMsg)
        {
            Dictionary<string, List<string>> potentialServiceIds = GetPotentialServiceUris(configMsg);

            return GetValidServiceUris(potentialServiceIds);
        }

        private Dictionary<string, List<string>> GetPotentialServiceUris(IFudgeFieldContainer configMsg)
        {
            var potentialServiceIds = new Dictionary<string, List<string>>();
            foreach (var userDataField in configMsg)
            {
                if (!(userDataField.Value is IFudgeFieldContainer))
                {
                    continue;
                }

                var uris = new List<string>();
                foreach (var field in (IFudgeFieldContainer)userDataField.Value)
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
                            var uri = (string)field.Value;
                            Logger.Debug("Candidate service {0}-{1}", userDataField.Name, uri);
                            uris.Add(uri);
                            break;
                        default:
                            continue;
                    }
                }

                potentialServiceIds.Add(userDataField.Name, uris);
            }
            return potentialServiceIds;
        }

        private IDictionary<string, Uri> GetValidServiceUris(Dictionary<string, List<string>> potentialServiceIds)
        {
            var validServiceUris = new Dictionary<string, Uri>();

            var requestsByHttpRequest = new Dictionary<HttpWebRequest, Tuple<string, HttpWebRequest, IAsyncResult>>();
            using (var finishedRequests = new BlockingCollection<HttpWebRequest>())
            {
                foreach (var potentialServiceId in potentialServiceIds)
                {
                    var serviceId = potentialServiceId.Key;
                    foreach (var uri in potentialServiceId.Value)
                    {
                        var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                        webRequest.Method = "HEAD";
                        var result = webRequest.BeginGetResponse(delegate { finishedRequests.Add(webRequest); }, serviceId);
                        var tuple = new Tuple<string, HttpWebRequest, IAsyncResult>(serviceId, webRequest, result);
                        requestsByHttpRequest.Add(webRequest, tuple);
                    }
                }

                while (requestsByHttpRequest.Any() || finishedRequests.Any())
                {
                    HttpWebRequest completedHttpReq;
                    if (!finishedRequests.TryTake(out completedHttpReq, 5000))
                    {
                        //NOTE Can't use WaitHandle.WaitAny On some implementations, if more that 64 handles are passed, a NotSupportedException is thrown, see http://msdn.microsoft.com/en-us/library/cc189983.aspx
                        //Have to timeout by hand, see http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout.aspx
                        var requestLogMessage = string.Join(",",
                                                            requestsByHttpRequest.Values.Select(
                                                                a =>
                                                                string.Format("{0}-{1}", a.Item1, a.Item2.RequestUri)));
                        Logger.Warn("Timed out when choosing services: {0}", requestLogMessage);
                        foreach (var r in requestsByHttpRequest.Keys)
                        {
                            r.Abort();
                        }
                        continue;
                    }

                    Tuple<string, HttpWebRequest, IAsyncResult> completedRequest = requestsByHttpRequest[completedHttpReq];
                    requestsByHttpRequest.Remove(completedHttpReq);

                    if (IsValidResponse(completedRequest))
                    {
                        Logger.Info("Resolved {0} for service {1}", completedRequest.Item2.RequestUri, completedRequest.Item1);
                        validServiceUris[completedRequest.Item1] = completedRequest.Item2.RequestUri;

                        foreach (var req in requestsByHttpRequest.Values.Where(r => r.Item1 == completedRequest.Item1))
                        {
                            Logger.Debug("Ignoring candidate {0} for service {1}", completedRequest.Item2.RequestUri,
                                         completedRequest.Item1);
                            req.Item2.Abort();
                        }
                    }
                }

                if (!validServiceUris.Any())
                {
                    throw new WebException("Failed to get any service Uris");
                }
                var missingKeys = potentialServiceIds.Keys.Except(validServiceUris.Keys);
                Logger.Warn("Failed to load services {0}", missingKeys);
                return validServiceUris;
            }
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
        }
        #endregion
    }
}