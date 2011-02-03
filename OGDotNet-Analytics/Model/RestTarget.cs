using System;
using System.IO;
using System.Linq;
using System.Net;
using Fudge;

namespace OGDotNet_Analytics.Model
{
    public class RestTarget
    {
        private const string FudgeMimeType = "application/vnd.fudgemsg";

        private readonly Uri _serviceUri;

        public RestTarget(Uri serviceUri)
        {
            _serviceUri = serviceUri;
        }

        public RestTarget(string serviceUri)
        {
            _serviceUri = new Uri(serviceUri);
        }


        public RestTarget Resolve(string method, params Tuple<string,string>[] queryParams)
        {
            var safeMethod = Uri.EscapeDataString(method);
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, safeMethod);
            uriBuilder.Query = String.Join("&",
                                           queryParams.Select(
                                               p => string.Format("{0}={1}", Uri.EscapeDataString(p.Item1), Uri.EscapeDataString(p.Item2))));
            return new RestTarget(uriBuilder.Uri);
        }

        public FudgeMsg Post(FudgeContext context = null, FudgeMsg reqMsg = null)
        {
            using (HttpWebResponse response = PostImpl(context, reqMsg))
            using (Stream responseStream = response.GetResponseStream())
            using (BufferedStream buff = new BufferedStream(responseStream))
            {
                var fudgeContext = context ?? new FudgeContext();
                var fudgeMsgEnvelope = fudgeContext.Deserialize(buff);
                return fudgeMsgEnvelope == null ? null : fudgeMsgEnvelope.Message;
            }
        }

        public Uri Create(FudgeContext context = null, FudgeMsg reqMsg = null)
        {
            using (HttpWebResponse response = PostImpl(context, reqMsg))
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return new Uri(response.Headers["Location"]);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        private HttpWebResponse PostImpl(FudgeContext context = null, FudgeMsg reqMsg = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);
            MangleRequest(request);
            request.Method = "POST";

            if (context != null)
                using (var requestStream = request.GetRequestStream())
                {
                    context.Serialize(reqMsg, requestStream);
                }

            return (HttpWebResponse)request.GetResponse();
        }

        public FudgeMsg GetReponse()
        {
            var fudgeContext = new FudgeContext();

            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);

            MangleRequest(request);
            var response = (HttpWebResponse)request.GetResponse();
            if (response.ContentLength == 0)
            {
                return null;
            }

            FudgeMsg fudgeMsg;
            using (Stream responseStream = response.GetResponseStream())
            using (BufferedStream buff = new BufferedStream(responseStream))
            {
                fudgeMsg = fudgeContext.Deserialize(buff).Message;
            }
            return fudgeMsg;
        }

        private static void MangleRequest(HttpWebRequest request)
        {
            request.Accept = FudgeMimeType;
            request.ContentType = FudgeMimeType;
        }
    }
}