using System;

namespace OGDotNet_Analytics.Model.Resources
{
    public class RemoteClient
    {
        private readonly string _clientId;
        private readonly RestTarget _rest;

        public RemoteClient(RestTarget userDataRest)
            : this(userDataRest, Environment.UserName, Guid.NewGuid().ToString())
        {
        }

        public RemoteClient(RestTarget userDataRest, string username, string clientId)
        {
            _clientId = clientId;
            _rest = userDataRest.GetSubMagic(username).GetSubMagic("clients").GetSubMagic(_clientId);
        }


        public Action HeartbeatSender
        {
            get { return () => _rest.GetSubMagic("heartbeat").GetReponse("POST"); }
        }
    }
}