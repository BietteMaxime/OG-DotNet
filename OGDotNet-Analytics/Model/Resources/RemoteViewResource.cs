using System;
using System.Net;
using Fudge.Serialization;
using OGDotNet_Analytics.Mappedtypes.Core.Position;
using OGDotNet_Analytics.Mappedtypes.engine.View;
using OGDotNet_Analytics.Mappedtypes.LiveData;

namespace OGDotNet_Analytics.Model.Resources
{
    public class RemoteViewResource
    {
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;

        public RemoteViewResource(RestTarget rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public void Init()
        {
            var fudgeMsg = _rest.GetSubMagic("init").GetReponse("POST");
        }

        public Portfolio Portfolio
        {
            get
            {
                var fudgeMsg = _rest.GetSubMagic("portfolio").GetReponse();
                if (fudgeMsg == null)
                {
                    return null;
                }

                FudgeSerializer fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                return fudgeSerializer.Deserialize<Portfolio>(fudgeMsg);
            }
        }

        public ViewDefinition Definition
        {
            get
            {
                var fudgeMsg = _rest.GetSubMagic("definition").GetReponse();
                return FudgeConfig.GetFudgeSerializer().Deserialize<ViewDefinition>(fudgeMsg);
            }
        }

        public ViewClientResource CreateClient()
        {
            
            var clientUri = _rest.GetSubMagic("clients").Create(FudgeConfig.GetFudgeContext(), FudgeConfig.GetFudgeSerializer().SerializeToMsg(new UserPrincipal("bbgintegrationtestuser", GetIP())));

            return new ViewClientResource(clientUri, _activeMqSpec);
        }

        private string GetIP()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                return ipaddress.ToString();
            }
            throw new ArgumentException();
        }
    }
}