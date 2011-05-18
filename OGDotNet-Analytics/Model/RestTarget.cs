//-----------------------------------------------------------------------
// <copyright file="RestTarget.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.View.calc;

namespace OGDotNet.Model
{
    public class RestTarget
    {
        private const string FudgeMimeType = "application/vnd.fudgemsg";

        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _serviceUri;

        public RestTarget(OpenGammaFudgeContext fudgeContext, Uri serviceUri)
        {
            _fudgeContext = fudgeContext;
            _serviceUri = serviceUri;
        }

        public RestTarget Resolve(params string[] segments)
        {
            if (!segments.Any())
                return this;
            return Resolve(segments.First()).Resolve(segments.Skip(1).ToArray());
        }
        public RestTarget Resolve(string method, params Tuple<string, string>[] queryParams)
        {
            var safeMethod = Uri.EscapeDataString(method);
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, safeMethod);
            uriBuilder.Query = string.Join("&",
            queryParams.Select(p => string.Format("{0}={1}", Uri.EscapeDataString(p.Item1), Uri.EscapeDataString(p.Item2))));
            var serviceUri = uriBuilder.Uri;

            UriHacks.LeaveDotsAndSlashesEscaped(serviceUri);
            if (!_serviceUri.IsBaseOf(serviceUri))
            {
                throw new ArgumentException("Failed to resolve uri sensibly", "method");
            }
            if (serviceUri.Segments.Last() != safeMethod)
            {
                throw new ArgumentException("Failed to resolve uri sensibly", "method");
            }

            return new RestTarget(_fudgeContext, serviceUri);
        }

        public RestTarget Create(object reqObj)
        {
            FudgeSerializer fudgeSerializer = _fudgeContext.GetSerializer();
            var reqMsg = reqObj == null ? null : fudgeSerializer.SerializeToMsg(reqObj);

            using (HttpWebResponse response = RequestImpl("POST", reqMsg))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                    case HttpStatusCode.OK:
                        return new RestTarget(_fudgeContext, new Uri(response.Headers["Location"]));
                    case HttpStatusCode.NoContent:
                        return null;
                    default:
                        throw new ArgumentException(string.Format("Response was not understood {0}", response.StatusCode));
                }
            }
        }

        public TRet Get<TRet>()
        {
            FudgeMsg retMsg = GetFudge();
            return retMsg == null ? default(TRet) : Deserialize<TRet>(retMsg);
        }

        public FudgeMsg GetFudge()
        {
            return FudgeRequestImpl();
        }

        public TRet Post<TRet>(object reqObj)
        {
            FudgeMsg retMsg = Post(reqObj);
            return retMsg == null ? default(TRet) : Deserialize<TRet>(retMsg);
        }

        public TRet Post<TRet>(object reqObj, string subMessageField)
        {
            FudgeMsg retMsg = Post(reqObj);
            return ProjectSubMessage<TRet>(retMsg, subMessageField);
        }

        public FudgeMsg Post(object reqObj = null)
        {
            if (reqObj is IFudgeFieldContainer)
                throw new ArgumentException();

            if (reqObj == null)
            {
                return PostFudge(null);
            }
            else if (_fudgeContext.TypeDictionary.GetByCSharpType(reqObj.GetType()) != null)
            {
                var reqMsg = _fudgeContext.NewMessage(new Field("value", reqObj));
                return PostFudge(reqMsg);
            }
            else
            {
                var reqMsg = _fudgeContext.GetSerializer().SerializeToMsg(reqObj);
                return PostFudge(reqMsg);
            }
        }

        public FudgeMsg PostFudge(FudgeMsg reqMsg)
        {
            return FudgeRequestImpl("POST", reqMsg);
        }

        public void Post(string obj)
        {
            RestExceptionMapping.DoWithExceptionMapping(delegate
            {
                HttpWebRequest request = GetBasicRequest();
                request.Method = "POST";

                request.ContentType = null;
                using (var requestStream = request.GetRequestStream())
                using (var sw = new StreamWriter(requestStream))
                {
                    sw.Write(obj);
                }

                using (request.GetResponse())
                {
                }
            });
        }

        public void Delete()
        {
            RequestImpl("DELETE").Close();
        }

        public TRet Put<TRet>(object reqObj, string subMessageField)
        {
            var reqMsg = _fudgeContext.GetSerializer().SerializeToMsg(reqObj);
            FudgeMsg retMsg = FudgeRequestImpl("PUT", reqMsg);
            return ProjectSubMessage<TRet>(retMsg, subMessageField);
        }

        public void Put(FudgeMsg reqMsg = null)
        {
            RequestImpl("PUT", reqMsg).Close();
        }

        public TRet Get<TRet>(string subMessageField)
        {
            FudgeMsg retMsg = GetFudge();
            return ProjectSubMessage<TRet>(retMsg, subMessageField);
        }

        private FudgeMsg FudgeRequestImpl(string method = "GET", FudgeMsg reqMsg = null)
        {
            try
            {
                using (var response = RequestImpl(method, reqMsg))
                using (var responseStream = response.GetResponseStream())
                using (var buff = new BufferedStream(responseStream))
                {
                    switch (response.ContentType)
                    {
                        case FudgeMimeType:
                            var fudgeMsgEnvelope = _fudgeContext.Deserialize(buff);
                            if (fudgeMsgEnvelope == null)
                                return null;
                            return fudgeMsgEnvelope.Message;
                        case "":
                            return null;
                        default:
                            throw new Exception("Unexpected content type " + response.ContentType);
                    }
                }
            }
            catch (WebException e)
            {//See RestClient.getMsgEnvelope
                if (e.Response == null)
                    throw;
                var httpWebResponse = (HttpWebResponse)e.Response;
                switch (httpWebResponse.StatusCode)
                {
                    case HttpStatusCode.NoContent://204
                        return new FudgeMsg(_fudgeContext);
                    case HttpStatusCode.NotFound://404
                        return null;
                    default:
                        throw;
                }
            }
        }


        public string EncodeBean(object bean)
        {
            using (var stream = new MemoryStream())
            {
                var reqMsg = _fudgeContext.GetSerializer().SerializeToMsg(bean);
                _fudgeContext.Serialize(reqMsg, stream);
                var buffer = stream.GetBuffer();
                return Convert.ToBase64String(buffer, 0, (int) stream.Length, Base64FormattingOptions.None);
            }
        }

        private HttpWebResponse RequestImpl(string method = "GET", FudgeMsg reqMsg = null)
        {
            return RestExceptionMapping.GetWithExceptionMapping(delegate
            {
                HttpWebRequest request = GetBasicRequest();
                request.Method = method;

                if (reqMsg != null)
                {
                    using (var requestStream = request.GetRequestStream())
                    {
                        _fudgeContext.Serialize(reqMsg, requestStream);
                    }
                }

                return (HttpWebResponse)request.GetResponse();
            });
        }

        private HttpWebRequest GetBasicRequest()
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);
            request.Accept = FudgeMimeType;
            request.ContentType = FudgeMimeType;
            var uaAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            request.UserAgent = string.Format("{0}/{1}", uaAssembly.GetName().Name, uaAssembly.GetName().Version);
            return request;
        }

        private TRet ProjectSubMessage<TRet>(FudgeMsg retMsg, string subMessageField)
        {
            if (retMsg == null)
                return default(TRet);
            FudgeSerializer fudgeSerializer = _fudgeContext.GetSerializer();
            var subMessage = retMsg.GetMessage(subMessageField);
            if (subMessage == null)
                return default(TRet);
            return fudgeSerializer.Deserialize<TRet>((FudgeMsg)subMessage);
        }
        private TRet Deserialize<TRet>(FudgeMsg retMsg)
        {
            return _fudgeContext.GetSerializer().Deserialize<TRet>(retMsg);
        }

    }
}