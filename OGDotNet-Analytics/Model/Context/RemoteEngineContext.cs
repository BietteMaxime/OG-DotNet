using System;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContext
    {
        private readonly Config _config;

        internal RemoteEngineContext(Config config)
        {
            _config = config;
        }

        public Uri RootUri
        {
            get { return _config.RootUri; }
        }

        public RemoteClient CreateUserClient()
        {
            return new RemoteClient(new RestTarget(_config.UserDataUri));
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(new RestTarget(_config.ViewProcessorUri), _config.ActiveMQSpec);
            }
        }

        public RemoteSecuritySource SecuritySource
        {
            get {
                return new RemoteSecuritySource(new RestTarget(_config.SecuritySourceUri));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {//TODO this is a hack, should I even be exposing this?
            get
            {
                return new RemoteSecurityMaster(new RestTarget(_config.SecuritySourceUri.ToString().Replace("securitySource", "securityMaster")));
            }
        }

       
    }
}