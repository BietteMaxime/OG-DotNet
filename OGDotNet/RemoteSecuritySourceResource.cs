using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Fudge;
using Fudge.Serialization;
using Fudge.Serialization.Reflection;
using Fudge.Types;
using Fudge.Util;

namespace OGDotNet
{
    [Serializable]
    internal class SecurityDocument
    {
        //TODO public UniqueIdentifier UniqueId;
        public ManageableSecurity Security;

        public override string ToString()
        {
            return Security.ToString();
        }
    }

    [Serializable]
    internal class FinancialSecurity : ManageableSecurity
    {
    }

    [Serializable]
    public class ManageableSecurity
    {
        public string NameProp { get { return Name; } }
        public string SecurityTypeProp { get { return SecurityType; } }
        public string Name;
        public string SecurityType;
        public UniqueIdentifier UniqueId;

        public override string ToString()
        {
            return "ManageableSecurity " + Name;
        }
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
        public readonly string Type;

        public SecuritySearchRequest(PagingRequest pagingRequest, string name, string type)
        {
            PagingRequest = pagingRequest;
            Name = name;
            Type = type ?? "";
        }
    }

    
    /// <summary>
    /// There must be a better place for this
    /// </summary>
    internal static class FudgeEncoder
    {
        internal static FudgeMsg Encode(SecuritySearchRequest ssr)
        {

            return new FudgeMsg
                       {
                           {"name", ssr.Name},
                           {"type", null, StringFieldType.Instance, ssr.Type},
                           {"pagingRequest", Encode(ssr.PagingRequest)}
                       };
        }

        private static FudgeMsg Encode(PagingRequest pagingRequest)
        {
            return new FudgeMsg()
                       {
                           {"page", pagingRequest.page},
                           {"pagingSize", pagingRequest.pagingSize}
                       };
        }
    }
    

    class RemoteSecurityMaster
    {
        internal readonly RESTMagic _restMagic;

        public RemoteSecurityMaster(RESTMagic restMagic)
        {
            _restMagic = restMagic;
        }

        public ISecurity GetSecurity(UniqueIdentifier uid)
        {
            if (uid.IsVersioned)
            {
                throw new NotImplementedException();
            }


            FudgeMsg reponse = _restMagic.GetSubMagic("securities").GetSubMagic("security").GetSubMagic(uid.ToString()).GetReponse();

            var secField = reponse.GetByName("security");
            if (secField == null)
            {
                return null;// TODO not found semantics
            }
            return Security.FromFudgeMsg((FudgeMsg)secField.Value);
        }

        public Collection<ISecurity> getSecurities(IdentifierBundle bundle)
        {
            throw new NotImplementedException();
        }

        public ISecurity GetSecurity(IdentifierBundle bundle)
        {
            throw new NotImplementedException();
        }

        ///TODO public void Search(SecuritySearchRequest ssr)
        public IEnumerable<SecurityDocument> Search(string name, string type)
        {
            var fudgeContext = new FudgeContext();

            var mapper = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.master.security");
            var mapper2 = new JavaTypeMappingStrategy("OGDotNet", "com.opengamma.financial.security");

            fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty, new JoiningMappingStrategty(mapper, mapper2));
            fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);

            
            //var fullDetail= new FudgeMsg(fudgeContext);
            var request = new SecuritySearchRequest(new PagingRequest(1,10), name, type);

            FudgeSerializer fudgeSerializer = new FudgeSerializer(fudgeContext);
            var msg = fudgeSerializer.SerializeToMsg(request);
            var fudgeMsg = _restMagic.GetSubMagic("search").GetReponse(fudgeContext, msg);




            var paging = fudgeSerializer.Deserialize((FudgeMsg)fudgeMsg.GetByName("paging").Value, typeof(Paging));
            //List of documents
            var listOfDocsMessage = (FudgeMsg)fudgeMsg.GetByName("documents").Value;


            return listOfDocsMessage.GetAllFields().Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize).Cast<SecurityDocument>();
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

    class RESTMagic
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
            var uriBuilder = new UriBuilder(_serviceUri);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, method);
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

            var response = (HttpWebResponse)request.GetResponse();

            FudgeMsg fudgeMsg;
            using (Stream responseStream = response.GetResponseStream())
            {

                fudgeMsg = context.Deserialize(responseStream).Message;
            }
            return fudgeMsg;
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
            return method.Replace(":", "%3");
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
