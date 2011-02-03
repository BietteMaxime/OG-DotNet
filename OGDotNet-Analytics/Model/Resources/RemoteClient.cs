using System;

namespace OGDotNet.Model.Resources
{
    public class RemoteClient
    {
        private readonly string _clientId;
        private readonly RestTarget _rest;

        public RemoteClient(RestTarget userDataRest)
            : this(userDataRest, Environment.UserName, Guid.NewGuid().ToString())
        {
        }

        private RemoteClient(RestTarget userDataRest, string username, string clientId)
        {
            _clientId = clientId;
            _rest = userDataRest.Resolve(username).Resolve("clients").Resolve(_clientId);
        }


        public Action HeartbeatSender
        {
            get { return () => _rest.Resolve("heartbeat").Post(); }
        }
    }
}