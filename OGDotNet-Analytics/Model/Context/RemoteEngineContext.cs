using System;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContext
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Config _config;

        internal RemoteEngineContext(OpenGammaFudgeContext fudgeContext, Config config)
        {
            _fudgeContext = fudgeContext;
            _config = config;
        }

        public Uri RootUri
        {
            get { return _config.RootUri; }
        }

        public RemoteClient CreateUserClient()
        {
            return new RemoteClient(new RestTarget(_fudgeContext, _config.UserDataUri));
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(_fudgeContext, new RestTarget(_fudgeContext, _config.ViewProcessorUri), _config.ActiveMQSpec);
            }
        }

        public ISecuritySource SecuritySource
        {
            get {
                return new RemoteSecuritySource(_fudgeContext, new RestTarget(_fudgeContext, _config.SecuritySourceUri));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {//TODO this is a hack, should I even be exposing this?
            get
            {
                return new RemoteSecurityMaster(new RestTarget(_fudgeContext, _config.SecuritySourceUri.ToString().Replace("securitySource", "securityMaster")));
            }
        }

       
    }
}