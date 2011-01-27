using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet
{
    [Serializable]
    internal class SecurityDocument
    {
        public string UniqueId;
        public ManageableSecurity Security;

        public override string ToString()
        {
            return Security.ToString();
        }
    }

    public class FinancialSecurity : ManageableSecurity
    {
    }

    public class ManageableSecurity : Security
    {
    }

    [Serializable]
    internal class PagingRequest
    {
        public readonly int page;
        public readonly int pagingSize;

        public PagingRequest(int page, int pagingSize)
        {
            this.page = page;
            this.pagingSize = pagingSize;
        }
    }

    [Serializable]
    internal class SecuritySearchRequest
    {
        public readonly PagingRequest PagingRequest;
        public readonly string Name;
        public readonly string SecurityType;

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string securityType)
        {
            PagingRequest = pagingRequest;
            Name = name;
            SecurityType = securityType ?? "";
        }
    }


    class SearchResults<TDocument> //where TDocument extends Document
    {
        public Paging Paging { get; set; }
        public IList<TDocument> Documents { get; set; }
    }
    class RemoteSecurityMaster
    {
        internal readonly RESTMagic _restMagic;

        public RemoteSecurityMaster(RESTMagic restMagic)
        {
            _restMagic = restMagic;
        }
        
        public SearchResults<SecurityDocument> Search(string name, string type, PagingRequest pagingRequest)
        {
            var request = new SecuritySearchRequest(pagingRequest, name, type);
            
            FudgeSerializer fudgeSerializer = new FudgeSerializer(FudgeContext);
            var msg = fudgeSerializer.SerializeToMsg(request);
            var fudgeMsg = _restMagic.GetSubMagic("search").GetReponse(FudgeContext, msg);


            return fudgeSerializer.Deserialize<SearchResults<SecurityDocument>>(fudgeMsg);
        }

        private static FudgeContext FudgeContext
        {
            get
            {
                var fudgeContext = new FudgeContext();

                var mapper = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.master.security");
                var mapper2 = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.financial.security");
                var mapper3 = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.id");
                var mapper4 = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.financial.security.bond");
                var joiningMappingStrategty = new JoiningMappingStrategty(mapper, mapper2, mapper3, mapper4);
                fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty,
                                         joiningMappingStrategty);
                fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
                return fudgeContext;
            }
        }

        public ManageableSecurity GetSecurity(UniqueIdentifier uid)
        {
            var fudgeMsg = _restMagic.GetSubMagic("security").GetSubMagic(uid.ToString()).GetReponse();
            FudgeSerializer fudgeSerializer = new FudgeSerializer(FudgeContext);
            return fudgeSerializer.Deserialize<SecurityDocument>(fudgeMsg).Security;

        }
    }

    public class JoiningMappingStrategty : IFudgeTypeMappingStrategy
    {
        private readonly IFudgeTypeMappingStrategy[] _subStrategies;

        public JoiningMappingStrategty(params IFudgeTypeMappingStrategy[] subStrategies)
        {
            _subStrategies = subStrategies;
        }

        public string GetName(Type type)
        {
            return _subStrategies.Select(strat => strat.GetName(type)).Where(x => x != null).FirstOrDefault();
        }

        public Type GetType(string name)
        {
            return _subStrategies.Select(strat => strat.GetType(name)).Where(x => x != null).FirstOrDefault();
        }
    }
    public class B : ISurrogateSelector
    {

        public void ChainSelector(ISurrogateSelector selector)
        {
            
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            throw new NotImplementedException();
        }

        public ISurrogateSelector GetNextSelector()
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class Paging
    {

        public int Page;
        public int PagingSize;
        public int TotalItems;

        public int CurrentPage
        {
            get{return Page;}
        }
        public int Pages
        {
            get { return ((TotalItems -1)/PagingSize) + 1; }
        }
    }

    class RemoteSecurityMasterResource 
    {
        private RESTMagic _restMagic;

        public RemoteSecurityMasterResource(string baseUri)
        {
            _restMagic = new RESTMagic(baseUri).GetSubMagic("securityMaster");
        }

        public IEnumerable<RemoteSecurityMaster> GetSecurityMasters()
        {
            FudgeMsg reponse = _restMagic.GetReponse();
            IList<IFudgeField> fudgeFields = reponse.GetAllFields();

            return fudgeFields.Select(fudgeField => (string)fudgeField.Value).Select(GetSecurityMaster);
        }

        public RemoteSecurityMaster GetSecurityMaster(string uid)
        {
            return new RemoteSecurityMaster(_restMagic.GetSubMagic(uid));
        }


    }

    public class RESTMagic
    {

        private static string FUDGE_MIME_TYPE = "application/vnd.fudgemsg";
        //private static string s_accept = ("Accept", FUDGE_MIME_TYPE);
        //private static string s_contentType = ("Content-Type", FUDGE_MIME_TYPE);

        private readonly Uri _serviceUri;

        public RESTMagic(Uri serviceUri)
        {
            _serviceUri = serviceUri;
        }

        public RESTMagic(string serviceUri)
        {
            _serviceUri = new Uri(serviceUri);
        }

        public RESTMagic GetSubMagic(string method)
        {
            var safeMethod = UrlEncode(method);
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, safeMethod);
            return new RESTMagic(uriBuilder.Uri);
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
            request.Method = UrlEncode(method);

            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(body);
            }

            var response = (HttpWebResponse)request.GetResponse();

            return GetString(response);

        }

        private string UrlEncode(string method)
        {
            //TODO This
            return method.Replace(":", "%3").Replace(" ","%20");
        }

        static string GetString(HttpWebResponse response)
        {
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public FudgeMsg GetReponse()
        {
            var fudgeContext = new FudgeContext();

            var request = (HttpWebRequest)WebRequest.Create(_serviceUri);

            MangleRequest(request);
            var response = (HttpWebResponse)request.GetResponse();
            FudgeMsg fudgeMsg;
            using (Stream responseStream = response.GetResponseStream())
            {

                fudgeMsg = fudgeContext.Deserialize(responseStream).Message;
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
