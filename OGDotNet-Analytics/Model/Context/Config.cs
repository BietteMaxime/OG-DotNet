using System;

namespace OGDotNet.Model.Context
{
    public class Config
    {
        private readonly Uri _rootUri;
        private readonly string _activeMQSpec;
        private readonly Uri _userDataUri;
        private readonly Uri _viewProcessorUri;
        private readonly Uri _securitySourceUri;

        public Config(Uri rootUri, string activeMQSpec, Uri userDataUri, Uri viewProcessorUri, Uri securitySourceUri)
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

        public Uri UserDataUri
        {
            get { return _userDataUri; }
        }

        public Uri ViewProcessorUri
        {
            get { return _viewProcessorUri; }
        }

        public Uri SecuritySourceUri
        {
            get { return _securitySourceUri; }
        }
    }
}