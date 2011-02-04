using System;

namespace OGDotNet.Model.Context
{
    public class Config
    {
        private readonly Uri _rootUri;
        private readonly string _activeMQSpec;
        private readonly string _userDataUri;
        private readonly string _viewProcessorUri;
        private readonly string _securitySourceUri;

        public Config(Uri rootUri, string activeMQSpec, string userDataUri, string viewProcessorUri, string securitySourceUri)
        {
            _rootUri = rootUri;
            _activeMQSpec = activeMQSpec;
            _userDataUri = userDataUri;
            _viewProcessorUri = viewProcessorUri;
            _securitySourceUri = securitySourceUri;
        }

        public Uri RootUri
        {
            get { return _rootUri; }
        }

        public string ActiveMQSpec
        {
            get { return _activeMQSpec; }
        }

        public string UserDataUri
        {
            get { return _userDataUri; }
        }

        public string ViewProcessorUri
        {
            get { return _viewProcessorUri; }
        }

        public string SecuritySourceUri
        {
            get { return _securitySourceUri; }
        }
    }
}