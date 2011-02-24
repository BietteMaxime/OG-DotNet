using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Model
{
    public class RestTarget
    {
        private const string FudgeMimeType = "application/vnd.fudgemsg";

        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _serviceUri;

        public RestTarget(OpenGammaFudgeContext fudgeContext, string serviceUri) :this(fudgeContext, new Uri(serviceUri))
        {
            
        }
        public RestTarget(OpenGammaFudgeContext fudgeContext, Uri serviceUri)
        {
            _fudgeContext = fudgeContext;
            _serviceUri = serviceUri;
        }

        public RestTarget Resolve(string method, params Tuple<string,string>[] queryParams)
        {
            var safeMethod = Uri.EscapeDataString(method);
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, safeMethod);
            uriBuilder.Query = String.Join("&",
            queryParams.Select(p => string.Format("{0}={1}", Uri.EscapeDataString(p.Item1), Uri.EscapeDataString(p.Item2))));
            var serviceUri = uriBuilder.Uri;

            UriHacks.LeaveDotsAndSlashesEscaped(serviceUri);
            Debug.Assert(_serviceUri.IsBaseOf(serviceUri));
            Debug.Assert(serviceUri.Segments.Last() == safeMethod);
            
            return new RestTarget(_fudgeContext, serviceUri);
        }


        public RestTarget Create(object reqObj)
        {
            FudgeSerializer fudgeSerializer = _fudgeContext.GetSerializer();
            var reqMsg = fudgeSerializer.SerializeToMsg(reqObj);

            using (HttpWebResponse response = RequestImpl("POST", reqMsg))
            {
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new ArgumentException(string.Format("Response was not created {0}", response.StatusCode));
                }

                return new RestTarget(_fudgeContext, new Uri(response.Headers["Location"]));
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
            return Deserialize<TRet>((FudgeMsg)retMsg.GetMessage(subMessageField));
        }

        public FudgeMsg Post(object reqObj = null)
        {
            if (reqObj is IFudgeFieldContainer)
                throw new ArgumentException();

            if (reqObj == null)
            {
                return PostFudge(null);
            }
            else
            {
                var reqMsg = _fudgeContext.GetSerializer().SerializeToMsg(reqObj);
                return PostFudge(reqMsg);    
            }
        }




        private FudgeMsg PostFudge(FudgeMsg reqMsg)
        {
            return FudgeRequestImpl("POST", reqMsg);
        }

        public void Post(string obj)
        {
            RestExceptionMapping.DoWithExceptionMapping(delegate()
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
            RequestImpl("DELETE");
        }

        public void Put(FudgeMsg reqMsg = null)
        {
            RequestImpl("PUT", reqMsg);
        }

        public TRet Get<TRet>(string subMessageField)
        {
            FudgeSerializer fudgeSerializer = _fudgeContext.GetSerializer();
            FudgeMsg retMsg = GetFudge();
            if (retMsg== null)
                return default(TRet);
            var subMessage = retMsg.GetMessage(subMessageField);
            if (subMessage == null)
                return default(TRet);
            return fudgeSerializer.Deserialize<TRet>((FudgeMsg)subMessage);
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
                var httpWebResponse = (HttpWebResponse) e.Response;
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

        private HttpWebResponse RequestImpl(string method = "GET", FudgeMsg reqMsg = null)
        {
            return RestExceptionMapping.GetWithExceptionMapping(delegate()
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
            var uaAssembly = Assembly.GetEntryAssembly()  ?? Assembly.GetExecutingAssembly();
            request.UserAgent = string.Format("{0}/{1}", uaAssembly.GetName().Name, uaAssembly.GetName().Version);
            return request;
        }

        private TRet Deserialize<TRet>(FudgeMsg retMsg)
        {
            return _fudgeContext.GetSerializer().Deserialize<TRet>(retMsg);
        }
    }
}