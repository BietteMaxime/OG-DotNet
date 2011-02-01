using System;
using System.IO;
using System.Linq;
using System.Net;
using Fudge;

namespace OGDotNet_Analytics.Model
{
    public class RestTarget
    {

        private const string FUDGE_MIME_TYPE = "application/vnd.fudgemsg";
        //private static string s_accept = ("Accept", FUDGE_MIME_TYPE);
        //private static string s_contentType = ("Content-Type", FUDGE_MIME_TYPE);

        private readonly Uri _serviceUri;

        public RestTarget(Uri serviceUri)
        {
            _serviceUri = serviceUri;
        }

        public RestTarget(string serviceUri)
        {
            _serviceUri = new Uri(serviceUri);
        }


        public RestTarget GetSubMagic(string method, params Tuple<string,string>[] queryParams)
        {
            var safeMethod = Uri.EscapeDataString(method);
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, safeMethod);
            uriBuilder.Query = String.Join("&",
                                           queryParams.Select(
                                               p => string.Format("{0}={1}", Uri.EscapeDataString(p.Item1), Uri.EscapeDataString(p.Item2))));
            return new RestTarget(uriBuilder.Uri);
        }

        public string GetReponse(string method)
        {
            return GetReponse(method, "");
        }

        
        public FudgeMsg GetReponse(FudgeContext context, FudgeMsg reqMsg)
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUri.ToString().Replace("securitySource","securityMaster"));
            MangleRequest(request);

            request.Method = "POST";
            using (var requestStream = request.GetRequestStream())
            {
                context.Serialize(reqMsg, requestStream);
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                return  context.Deserialize(responseStream).Message;
            }
        }

        public Uri Create(FudgeContext context, FudgeMsg reqMsg)
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUri.ToString().Replace("securitySource", "securityMaster"));
            MangleRequest(request);

            request.Method = "POST";
            using (var requestStream = request.GetRequestStream())
            {
                context.Serialize(reqMsg, requestStream);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new ArgumentException();
                }
                return new Uri(response.Headers["Location"]);
            }
        }

        public string GetReponse(string method, string body)
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);
            MangleRequest(request);
            request.Method = Uri.EscapeDataString(method);

            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(body);
            }

            var response = (HttpWebResponse)request.GetResponse();

            return GetString(response);

        }

        static string GetString(HttpWebResponse response)
        {
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public FudgeMsg GetFudgeReponse(string method)
        {
            var fudgeContext = new FudgeContext();

            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);
            request.Method = Uri.EscapeDataString(method);

            MangleRequest(request);
            var response = (HttpWebResponse)request.GetResponse();
            FudgeMsg fudgeMsg;
            using (Stream responseStream = response.GetResponseStream())
            using (BufferedStream buff = new BufferedStream(responseStream))
            {

                fudgeMsg = fudgeContext.Deserialize(buff).Message;
            }
            return fudgeMsg;
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
            request.Accept = FUDGE_MIME_TYPE;
            request.ContentType = FUDGE_MIME_TYPE;
        }
    }
}